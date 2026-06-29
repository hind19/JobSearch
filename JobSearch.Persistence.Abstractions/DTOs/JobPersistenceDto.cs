// DTOs/JobPersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

public class JobPersistenceDto
{
    public Guid Id { get; }
    public Guid JobSiteId { get; }
    public string ExternalId { get; }
    public string Url { get; }
    public string Title { get; }
    public string Company { get; }
    public string? Location { get; }
    public string? SalaryRaw { get; }
    public string DescriptionRaw { get; }
    public DateTime? PostedAt { get; }
    public DateTime FoundAt { get; }
    public string UrlHash { get; }

    public JobPersistenceDto(
        Guid id,
        Guid jobSiteId,
        string externalId,
        string url,
        string title,
        string company,
        string? location,
        string? salaryRaw,
        string descriptionRaw,
        DateTime? postedAt,
        DateTime foundAt,
        string urlHash)
    {
        Id = id;
        JobSiteId = jobSiteId;
        ExternalId = externalId;
        Url = url;
        Title = title;
        Company = company;
        Location = location;
        SalaryRaw = salaryRaw;
        DescriptionRaw = descriptionRaw;
        PostedAt = postedAt;
        FoundAt = foundAt;
        UrlHash = urlHash;
    }
}