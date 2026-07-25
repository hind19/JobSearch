// JobSearch.Application.Abstractions/DTOs/SentEmailDto.cs
using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.Application.Abstractions.DTOs;

public class SentEmailDto(
    Guid id,
    Guid userId,
    string toAddress,
    string subject,
    string body,
    EmailSendStatus status,
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
    public EmailSendStatus Status { get; } = status;
    public int AttemptCount { get; } = attemptCount;
    public string? ErrorMessage { get; } = errorMessage;
    public DateTime? SentAt { get; } = sentAt;
    public DateTime CreatedAt { get; } = createdAt;
}
