// Interfaces/IUserJobRejectionRepository.cs — ADR-0009
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface IUserJobRejectionRepository
{
    Task<UserJobRejectionPersistenceDto> CreateAsync(
        UserJobRejectionPersistenceDto rejection,
        CancellationToken ct = default);

    // Single calendar day (in UTC, matching AnalyzedAt's storage), not a
    // range — the UI filter is "pick a day," not "pick an interval"
    // (ADR-0009). page is 1-based.
    Task<RejectedJobsPagePersistenceDto> GetByUserIdAndDateAsync(
        Guid userId,
        DateTime date,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // Backs "auto-load the most recent scan on open" — null if the user
    // has no rejections at all yet.
    Task<DateTime?> GetMostRecentAnalyzedDateAsync(
        Guid userId,
        CancellationToken ct = default);
}
