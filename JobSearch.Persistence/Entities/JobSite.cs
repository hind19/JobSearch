// Entities/JobSite.cs
namespace JobSearch.Persistence.Entities;

public class JobSite
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string ScrapeConfig { get; set; } = "{}";

    public List<Job> Jobs { get; set; } = [];
}