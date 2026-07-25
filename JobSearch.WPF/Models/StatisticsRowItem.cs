namespace JobSearch.WPF.Models;

public class StatisticsRowItem
{
    public string JobSiteName { get; init; } = string.Empty;
    public int JobsScrapedCount { get; init; }
    public int MatchesCount { get; init; }
    public string MatchRateDisplay { get; init; } = "—";
    public string AverageScoreDisplay { get; init; } = "—";
    public string MostRecentMatchDisplay { get; init; } = "—";
}
