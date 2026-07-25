// JobSearch.Business/Services/EmailAuditLog.cs
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Business.Services;

internal sealed class EmailAuditLog : IEmailAuditLog
{
    private readonly ISentEmailRepository _sentEmailRepository;

    public EmailAuditLog(ISentEmailRepository sentEmailRepository)
    {
        _sentEmailRepository = sentEmailRepository;
    }

    public async Task<Guid> RecordPendingAsync(
        Guid userId,
        string toAddress,
        string subject,
        string body,
        CancellationToken ct = default)
    {
        var dto = new SentEmailPersistenceDto(
            id: Guid.NewGuid(),
            userId: userId,
            toAddress: toAddress,
            subject: subject,
            body: body,
            status: "Pending",
            attemptCount: 0,
            errorMessage: null,
            sentAt: null,
            createdAt: DateTime.UtcNow);

        var created = await _sentEmailRepository.CreateAsync(dto, ct);
        return created.Id;
    }

    public async Task RecordResultAsync(
        Guid sentEmailId,
        bool sent,
        int attemptCount,
        string? errorMessage,
        CancellationToken ct = default) =>
        await _sentEmailRepository.UpdateStatusAsync(
            id: sentEmailId,
            status: sent ? "Sent" : "Failed",
            attemptCount: attemptCount,
            errorMessage: errorMessage,
            sentAt: sent ? DateTime.UtcNow : null,
            ct: ct);
}
