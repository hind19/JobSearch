using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace JobSearch.Email;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ADR-0005 §1: bound here only as seed data for the DB row on
        // first run — not read at send time afterwards (see
        // EmailSettingsService.GetAsync in JobSearch.Business).
        services.Configure<EmailSettingsSeedOptions>(
            configuration.GetSection("EmailSettings"));

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<EmailTemplateBuilder>();

        // ADR-0005 §4: 3 attempts, exponential backoff. Auth failures are
        // deliberately excluded — a config problem, not a transient one.
        services.AddResiliencePipeline("email-send", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = new PredicateBuilder()
                    .Handle<SmtpCommandException>()
                    .Handle<SmtpProtocolException>()
                    .Handle<IOException>()
                    .Handle<TimeoutException>()
            });
        });

        return services;
    }
}