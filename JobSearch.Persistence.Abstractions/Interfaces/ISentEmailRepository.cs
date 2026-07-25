// Interfaces/ISentEmailRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface ISentEmailRepository
{
    // ADR-0005: inserted as Pending before the send attempt, so a crash
    // mid-send still leaves an audit trail.
    Task<SentEmailPersistenceDto> CreateAsync(
        SentEmailPersistenceDto sentEmail,
        CancellationToken ct = default);

    Task UpdateStatusAsync(
        Guid id,
        string status,
        int attemptCount,
        string? errorMessage,
        DateTime? sentAt,
        CancellationToken ct = default);
}
