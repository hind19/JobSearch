// JobSearch.Application.Abstractions/Interfaces/IJobRejectionService.cs
// ADR-0009
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// WPF-facing only, same posture as IJobStatisticsService (ADR-0006) —
// JobSearch.Worker has no reason to consult this mid-run.
public interface IJobRejectionService
{
    Task<DateTime?> GetMostRecentAnalysisDateAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<RejectedJobsPageDto> GetRejectedJobsAsync(
        Guid userId,
        DateTime date,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
