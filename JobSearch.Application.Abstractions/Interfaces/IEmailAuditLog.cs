// JobSearch.Application.Abstractions/Interfaces/IEmailAuditLog.cs
namespace JobSearch.Application.Abstractions.Interfaces;

// Lets JobSearch.Email record send attempts (ADR-0005 §3) without taking
// a project dependency on Persistence.Abstractions — Email stays a leaf
// project, same as it was designed originally. Implemented in
// JobSearch.Business, wrapping ISentEmailRepository.
public interface IEmailAuditLog
{
    // Called before the send attempt — returns the new row's id so the
    // result can be recorded against it afterwards.
    Task<Guid> RecordPendingAsync(
        Guid userId,
        string toAddress,
        string subject,
        string body,
        CancellationToken ct = default);

    Task RecordResultAsync(
        Guid sentEmailId,
        bool sent,
        int attemptCount,
        string? errorMessage,
        CancellationToken ct = default);
}
