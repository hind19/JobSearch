// DTOs/JobSitePersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

public class JobSitePersistenceDto
{
    public Guid Id { get; }
    public string Name { get; }
    public string BaseUrl { get; }
    public bool IsActive { get; }
    public string ScrapeConfig { get; }

    public JobSitePersistenceDto(
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