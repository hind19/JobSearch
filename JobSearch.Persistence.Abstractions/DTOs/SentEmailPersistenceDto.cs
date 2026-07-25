// JobSearch.Persistence.Abstractions/DTOs/SentEmailPersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

public class SentEmailPersistenceDto(
    Guid id,
    Guid userId,
    string toAddress,
    string subject,
    string body,
    string status,
    int attemptCount,
    string? errorMessage,
    DateTime? sentAt,
    DateTime createdAt)
{
    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public string ToAddress { get; } = toAddress;
    public string Subject { get; } = subject;
    public string Body { get; } = body;
    public string Status { get; } = status;
    public int AttemptCount { get; } = attemptCount;
    public string? ErrorMessage { get; } = errorMessage;
    public DateTime? SentAt { get; } = sentAt;
    public DateTime CreatedAt { get; } = createdAt;
}
