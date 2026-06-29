namespace JobSearch.Application.Abstractions.DTOs;

public class JobSiteDto
{
    public Guid Id { get; }
    public string Name { get; }
    public string BaseUrl { get; }
    public bool IsActive { get; }
    public string ScrapeConfig { get; }

    public JobSiteDto(
        Guid id,
        string name,
        string baseUrl,
        bool isActive,
        string scrapeConfig)
    {
        Id = id;
        Name = name;
        BaseUrl = baseUrl;
        IsActive = isActive;
        ScrapeConfig = scrapeConfig;
    }
}