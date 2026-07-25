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
    JobPersistenceDto job,
    // ADR-0007: persistence foundation only. Optional with defaults so
    // every existing call site (JobMatchService.TryCreateMatchAsync,
    // PersistenceMapper) keeps compiling unmodified — nothing sets these
    // to anything but the defaults yet.
    bool isApplied = false,
    DateTime? appliedAt = null)
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
    public bool IsApplied { get; } = isApplied;
    public DateTime? AppliedAt { get; } = appliedAt;
}
