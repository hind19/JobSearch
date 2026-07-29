// ADR-0009
namespace JobSearch.WPF.Models;

public class RejectedJobRowItem
{
    public string JobUrl { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string RelevanceReasonDisplay { get; init; } = "—";
    public string AnalyzedAtDisplay { get; init; } = "—";
}
