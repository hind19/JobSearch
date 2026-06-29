// Entities/UserProfile.cs
namespace JobSearch.Persistence.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ClaudeReadyProfile { get; set; } = string.Empty;
    public string DesiredRoles { get; set; } = string.Empty;
    public int? DesiredSalaryMin { get; set; }
    public int? DesiredSalaryMax { get; set; }
    public string SalaryCurrency { get; set; } = "USD";
    public string LocationPreference { get; set; } = string.Empty;
    public DateTime CvParsedAt { get; set; }
    public string CvFileHash { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}