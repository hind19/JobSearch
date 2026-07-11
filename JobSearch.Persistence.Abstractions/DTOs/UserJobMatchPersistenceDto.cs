namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserJobMatchPersistenceDto(
    Guid id,
    Guid userId,
    Guid jobId,
    decimal relevanceScore,
    string? relevanceReason,
    bool wasNotified,
    DateTime? notifiedAt,
    DateTime foundInRunAt,
    JobPersistenceDto job)
{
    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public Guid JobId { get; } = jobId;
    public decimal RelevanceScore { get; } = relevanceScore;
    public string? RelevanceReason { get; } = relevanceReason;
    public bool WasNotified { get; } = wasNotified;
    public DateTime? NotifiedAt { get; } = notifiedAt;
    public DateTime FoundInRunAt { get; } = foundInRunAt;
    public JobPersistenceDto Job { get; } = job;
}
