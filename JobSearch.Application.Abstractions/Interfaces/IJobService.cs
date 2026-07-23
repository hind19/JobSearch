// JobSearch.Application.Abstractions/Interfaces/IJobService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// Read-side surface for JobSearch.WPF's (not yet built) job-browsing view.
// JobSearch.Worker never calls this — see IJobIngestService for its
// write-side needs instead. Kept separate per ISP (same reasoning as the
// IJobSiteService / IJobSiteQueryService split).
public interface IJobService
{
    Task<List<UserJobMatchDto>> GetMatchesByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);
}
