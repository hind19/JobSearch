namespace JobSearch.Persistence.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid JobSiteId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? SalaryRaw { get; set; }
    public string DescriptionRaw { get; set; } = string.Empty;
    public DateTime? PostedAt { get; set; }
    public DateTime FoundAt { get; set; }
    public string UrlHash { get; set; } = string.Empty;

    public JobSite JobSite { get; set; } = null!;
    public List<UserJobMatch> UserJobMatches { get; set; } = [];
}