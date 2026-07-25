// JobSearch.Persistence.Abstractions/DTOs/JobSiteStatisticsPersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

public class JobSiteStatisticsPersistenceDto(
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

    // Nullable: no matches yet means no average to report.
    public decimal? AverageRelevanceScore { get; } = averageRelevanceScore;

    // Nullable: no matches yet means no "most recent" date.
    public DateTime? MostRecentMatchAt { get; } = mostRecentMatchAt;
}
