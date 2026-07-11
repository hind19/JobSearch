namespace JobSearch.Application.Abstractions.DTOs;

public class UserJobMatchDto(
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
    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public Guid JobId { get; } = jobId;
    public decimal RelevanceScore { get; } = relevanceScore;
    public string? RelevanceReason { get; } = relevanceReason;
    public bool WasNotified { get; } = wasNotified;
    public DateTime? NotifiedAt { get; } = notifiedAt;
    public DateTime FoundInRunAt { get; } = foundInRunAt;
    public JobDto Job { get; } = job;
}
