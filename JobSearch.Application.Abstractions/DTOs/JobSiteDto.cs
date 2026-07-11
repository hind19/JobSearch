namespace JobSearch.Application.Abstractions.DTOs;

public class JobSiteDto(
    Guid id,
    string name,
    string baseUrl,
    bool isActive,
    string scrapeConfig)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string BaseUrl { get; } = baseUrl;
    public bool IsActive { get; } = isActive;
    public string ScrapeConfig { get; } = scrapeConfig;
}
