namespace JobSearch.Application.Abstractions.DTOs;

public class UserJobMatchDto
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid JobId { get; }
    public decimal RelevanceScore { get; }
    public string? RelevanceReason { get; }
    public bool WasNotified { get; }
    public DateTime? NotifiedAt { get; }
    public DateTime FoundInRunAt { get; }
    public JobDto Job { get; }

    public UserJobMatchDto(
        Guid id,
        Guid userId,
        Guid jobId,
        decimal relevanceScore,
        string? relevanceReason,
        bool wasNotified,
        DateTime? notifiedAt,
        DateTime foundInRunAt,
        JobDto job)
    {
        Id = id;
        UserId = userId;
        JobId = jobId;
        RelevanceScore = relevanceScore;
        RelevanceReason = relevanceReason;
        WasNotified = wasNotified;
        NotifiedAt = notifiedAt;
        FoundInRunAt = foundInRunAt;
        Job = job;
    }
}