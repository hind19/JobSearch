namespace JobSearch.Persistence.Abstractions.DTOs;

public class JobPersistenceDto(
    Guid id,
    Guid jobSiteId,
    string? externalId,
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
    public Guid Id { get; } = id;
    public Guid JobSiteId { get; } = jobSiteId;
    public string? ExternalId { get; } = externalId;
    public string Url { get; } = url;
    public string Title { get; } = title;
    public string Company { get; } = company;
    public string? Location { get; } = location;
    public string? SalaryRaw { get; } = salaryRaw;
    public string DescriptionRaw { get; } = descriptionRaw;
    public DateTime? PostedAt { get; } = postedAt;
    public DateTime FoundAt { get; } = foundAt;
    public string UrlHash { get; } = urlHash;
}
