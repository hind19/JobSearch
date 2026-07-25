// JobSearch.Application.Abstractions/Interfaces/IJobStatisticsService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// WPF-facing only (future StatisticsViewModel). JobSearch.Worker has no
// reason to consult this mid-run — it's a reporting feature for the
// human, not an input to scraping/matching logic (ADR-0006).
public interface IJobStatisticsService
{
    Task<List<JobSiteStatisticsDto>> GetStatisticsAsync(
        CancellationToken ct = default);
}
