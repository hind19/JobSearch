namespace JobSearch.Persistence.Entities;

public class UserJobMatch
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid JobId { get; set; }
    public decimal RelevanceScore { get; set; }
    public string? RelevanceReason { get; set; }
    public bool WasNotified { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public DateTime FoundInRunAt { get; set; }

    // ADR-0007: persistence foundation only — nothing sets these yet.
    public bool IsApplied { get; set; }
    public DateTime? AppliedAt { get; set; }

    public User User { get; set; } = null!;
    public Job Job { get; set; } = null!;
}