// Interfaces/IJobStatisticsRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

// ADR-0006: dedicated repository rather than a method on IJobRepository
// or IUserJobMatchRepository — the aggregate query (Jobs LEFT JOIN
// UserJobMatches, grouped by JobSiteId) spans both aggregate roots and
// doesn't belong to either one specifically.
public interface IJobStatisticsRepository
{
    // One method, no per-site filter or pagination — there are fewer
    // than 10 job sites (ADR-0003), so the WPF screen shows all of them
    // at once.
    Task<List<JobSiteStatisticsPersistenceDto>> GetStatisticsAsync(
        CancellationToken ct = default);
}
