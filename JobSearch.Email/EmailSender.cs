using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly.Registry;

namespace JobSearch.Email
{
    internal class EmailSender : IEmailSender
    {
        private const string ResiliencePipelineName = "email-send";

        private readonly IEmailSettingsService _emailSettingsService;
        private readonly IEmailAuditLog _emailAuditLog;
        private readonly EmailTemplateBuilder _templateBuilder;
        private readonly IConfiguration _configuration;
        private readonly ResiliencePipelineProvider<string> _pipelineProvider;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            IEmailSettingsService emailSettingsService,
            IEmailAuditLog emailAuditLog,
            EmailTemplateBuilder templateBuilder,
            IConfiguration configuration,
            ResiliencePipelineProvider<string> pipelineProvider,
            ILogger<EmailSender> logger)
        {
            _emailSettingsService = emailSettingsService;
            _emailAuditLog = emailAuditLog;
            _templateBuilder = templateBuilder;
            _configuration = configuration;
            _pipelineProvider = pipelineProvider;
            _logger = logger;
        }

        public async Task<EmailSendResult> SendJobDigestAsync(
            Guid userId,
            string toAddress,
            List<UserJobMatchDto> matches,
            CancellationToken ct = default)
        {
            var settings = await _emailSettingsService.GetAsync(ct);

            if (settings is null)
            {
                _logger.LogError(
                    "No email settings configured (DB empty, no seed in " +
                    "appsettings.json). Cannot send digest for user {UserId}.",
                    userId);

                return new EmailSendResult(
                    sent: false,
                    sentEmailId: Guid.Empty,
                    errorMessage: "Email settings not configured.");
            }

            var (subject, body) = _templateBuilder.BuildJobDigest(matches);

            // ADR-0005 §3: recorded as Pending before the send attempt, so
            // a crash mid-send still leaves an audit trail.
            var sentEmailId = await _emailAuditLog.RecordPendingAsync(
                userId, toAddress, subject, body, ct);

            var pipeline = _pipelineProvider.GetPipeline(ResiliencePipelineName);
            var attemptCount = 0;

            try
            {
                await pipeline.ExecuteAsync(async innerCt =>
                {
                    attemptCount++;
                    await SendSmtpAsync(settings, toAddress, subject, body, innerCt);
                }, ct);

                await _emailAuditLog.RecordResultAsync(
                    sentEmailId, sent: true, attemptCount, errorMessage: null, ct);

                return new EmailSendResult(
                    sent: true, sentEmailId: sentEmailId, errorMessage: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send job digest to user {UserId} after " +
                    "{AttemptCount} attempt(s).", userId, attemptCount);

                await _emailAuditLog.RecordResultAsync(
                    sentEmailId, sent: false, attemptCount, ex.Message, ct);

                return new EmailSendResult(
                    sent: false, sentEmailId: sentEmailId, errorMessage: ex.Message);
            }
        }

        private async Task SendSmtpAsync(
            EmailSettingsDto settings,
            string toAddress,
            string subject,
            string body,
            CancellationToken ct)
        {
            // ADR-0005 §2: password never comes from the DB — read
            // directly from configuration (user-secrets/env var) at send
            // time, same convention as AnthropicSettings:ApiKey.
            var password = _configuration["EmailSettings:SmtpPassword"]
                ?? throw new InvalidOperationException(
                    "SMTP password not found. Configure via user-secrets " +
                    "or environment variable 'EmailSettings__SmtpPassword'.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                settings.FromDisplayName, settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                settings.SmtpHost,
                settings.SmtpPort,
                settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                ct);

            if (!string.IsNullOrEmpty(settings.SmtpUsername))
                await client.AuthenticateAsync(settings.SmtpUsername, password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
