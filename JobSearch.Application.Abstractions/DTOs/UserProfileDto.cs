namespace JobSearch.Application.Abstractions.DTOs;

public class UserProfileDto
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string ClaudeReadyProfile { get; }
    public string DesiredRoles { get; }
    public int? DesiredSalaryMin { get; }
    public int? DesiredSalaryMax { get; }
    public string SalaryCurrency { get; }
    public string LocationPreference { get; }
    public DateTime CvParsedAt { get; }
    public string CvFileHash { get; }
    public DateTime UpdatedAt { get; }

    public UserProfileDto(
        Guid id,
        Guid userId,
        string claudeReadyProfile,
        string desiredRoles,
        int? desiredSalaryMin,
        int? desiredSalaryMax,
        string salaryCurrency,
        string locationPreference,
        DateTime cvParsedAt,
        string cvFileHash,
        DateTime updatedAt)
    {
        Id = id;
        UserId = userId;
        ClaudeReadyProfile = claudeReadyProfile;
        DesiredRoles = desiredRoles;
        DesiredSalaryMin = desiredSalaryMin;
        DesiredSalaryMax = desiredSalaryMax;
        SalaryCurrency = salaryCurrency;
        LocationPreference = locationPreference;
        CvParsedAt = cvParsedAt;
        CvFileHash = cvFileHash;
        UpdatedAt = updatedAt;
    }
}