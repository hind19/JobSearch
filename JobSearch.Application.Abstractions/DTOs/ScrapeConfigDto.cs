// JobSearch.Application.Abstractions/DTOs/ScrapeConfigDto.cs
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