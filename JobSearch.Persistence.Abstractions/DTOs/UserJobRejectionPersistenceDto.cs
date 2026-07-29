// ADR-0009
namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserJobRejectionPersistenceDto(
    Guid id,
    Guid userId,
    Guid jobId,
    decimal relevanceScore,
    string? relevanceReason,
    DateTime analyzedAt,
    JobPersistenceDto job)
{
    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public Guid JobId { get; } = jobId;
    public decimal RelevanceScore { get; } = relevanceScore;
    public string? RelevanceReason { get; } = relevanceReason;
    public DateTime AnalyzedAt { get; } = analyzedAt;
    public JobPersistenceDto Job { get; } = job;
}
