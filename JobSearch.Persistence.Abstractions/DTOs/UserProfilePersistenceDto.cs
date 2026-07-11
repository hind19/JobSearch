namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserProfilePersistenceDto(
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
    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public string ClaudeReadyProfile { get; } = claudeReadyProfile;
    public string DesiredRoles { get; } = desiredRoles;
    public int? DesiredSalaryMin { get; } = desiredSalaryMin;
    public int? DesiredSalaryMax { get; } = desiredSalaryMax;
    public string SalaryCurrency { get; } = salaryCurrency;
    public string LocationPreference { get; } = locationPreference;
    public DateTime CvParsedAt { get; } = cvParsedAt;
    public string CvFileHash { get; } = cvFileHash;
    public DateTime UpdatedAt { get; } = updatedAt;
}
