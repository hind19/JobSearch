// JobSearch.Application.Abstractions/DTOs/JobSiteStatisticsDto.cs
namespace JobSearch.Application.Abstractions.DTOs;

public class JobSiteStatisticsDto(
    Guid jobSiteId,
    string jobSiteName,
    int jobsScrapedCount,
    int matchesCount,
    decimal? averageRelevanceScore,
    DateTime? mostRecentMatchAt)
{
    public Guid JobSiteId { get; } = jobSiteId;
    public string JobSiteName { get; } = jobSiteName;
    public int JobsScrapedCount { get; } = jobsScrapedCount;
    public int MatchesCount { get; } = matchesCount;
    public decimal? AverageRelevanceScore { get; } = averageRelevanceScore;
    public DateTime? MostRecentMatchAt { get; } = mostRecentMatchAt;

    // Match rate is deliberately NOT a stored/persisted field — it's
    // derived (MatchesCount / JobsScrapedCount) and computed on demand
    // here rather than in the WPF ViewModel, so every consumer of this
    // DTO gets the same division-by-zero handling instead of each caller
    // reimplementing it.
    public decimal? MatchRate =>
        JobsScrapedCount == 0
            ? null
            : (decimal)MatchesCount / JobsScrapedCount;
}
