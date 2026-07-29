// ADR-0009: a job Claude analyzed and scored below RelevanceThreshold.
namespace JobSearch.Persistence.Entities;

public class JobRejection
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid JobId { get; set; }
    public decimal RelevanceScore { get; set; }
    public string? RelevanceReason { get; set; }
    public DateTime AnalyzedAt { get; set; }

    public User User { get; set; } = null!;
    public Job Job { get; set; } = null!;
}
