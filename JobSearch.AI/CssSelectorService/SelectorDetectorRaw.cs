// JobSearch.AI/Services/SelectorDetectorRaw.cs
namespace JobSearch.AI.Services;

internal sealed class SelectorDetectorRaw
{
    public string? ContainerSelector { get; init; }
    public string? LinkSelector { get; init; }
    public string? CompanySelector { get; init; }
    public string? SnippetSelector { get; init; }
    public string? DateSelector { get; init; }
}