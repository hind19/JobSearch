// JobSearch.Application.Abstractions/DTOs/JobSiteDto.cs
namespace JobSearch.Application.Abstractions.DTOs;

public class JobSiteDto(
    Guid id,
    string name,
    string baseUrl,
    bool isActive,
    ScrapeConfigDto scrapeConfig)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string BaseUrl { get; } = baseUrl;
    public bool IsActive { get; } = isActive;
    public ScrapeConfigDto ScrapeConfig { get; } = scrapeConfig;
}