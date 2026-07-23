// JobSearch.Application.Abstractions/DTOs/ScrapeConfigDto.cs
// TODO: CompanySelector, SnippetSelector and DateSelector are currently
// unused. Under the agent-loop architecture (see ADR-0004), job detail
// parsing (company, description, posting date) is done by Claude reading
// the full job page via fetch_job_page, not via listing-page selectors.
// These three fields would only be useful as a pre-filter to skip
// fetch_job_page for obviously irrelevant listings before spending a
// Claude call on them — remove during debugging/refactoring if that
// optimization isn't pursued.
public class ScrapeConfigDto(
    string searchUrlTemplate,
    string searchQuery,
    string? containerSelector,
    string? linkSelector,
    string? companySelector,
    string? snippetSelector,
    string? dateSelector)
{
    public string SearchUrlTemplate { get; } = searchUrlTemplate;
    public string SearchQuery { get; } = searchQuery;
    public string? ContainerSelector { get; } = containerSelector;
    public string? LinkSelector { get; } = linkSelector;
    public string? CompanySelector { get; } = companySelector;
    public string? SnippetSelector { get; } = snippetSelector;
    public string? DateSelector { get; } = dateSelector;

    public static ScrapeConfigDto Empty => new(
        string.Empty, string.Empty,
        null, null, null, null, null);
}