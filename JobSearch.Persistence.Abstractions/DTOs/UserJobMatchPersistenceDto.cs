// DTOs/UserJobMatchPersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserJobMatchPersistenceDto
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid JobId { get; }
    public decimal RelevanceScore { get; }
    public string? RelevanceReason { get; }
    public bool WasNotified { get; }
    public DateTime? NotifiedAt { get; }
    public DateTime FoundInRunAt { get; }
    public JobPersistenceDto Job { get; }

    public UserJobMatchPersistenceDto(
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