// ADR-0009 — flat fields only (no nested JobDto): this is a display DTO
// for a Grid row, not a general-purpose aggregate-carrying DTO.
namespace JobSearch.Application.Abstractions.DTOs;

public class RejectedJobDto(
    Guid id,
    string jobUrl,
    string jobTitle,
    decimal relevanceScore,
    string? relevanceReason,
    DateTime analyzedAt)
{
    public Guid Id { get; } = id;
    public string JobUrl { get; } = jobUrl;
    public string JobTitle { get; } = jobTitle;
    public decimal RelevanceScore { get; } = relevanceScore;
    public string? RelevanceReason { get; } = relevanceReason;
    public DateTime AnalyzedAt { get; } = analyzedAt;
}
