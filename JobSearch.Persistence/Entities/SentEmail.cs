// Entities/SentEmail.cs
namespace JobSearch.Persistence.Entities;

public class SentEmail
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
