// JobSearch.Application.Abstractions/Interfaces/IJobMatchService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

public interface IJobMatchService
{
    // Worker agent loop's score_relevance tool. The threshold comparison
    // happens inside the implementation (AnthropicSettings:RelevanceThreshold),
    // in C# — not left to the model's own judgment (ADR-0004 guardrail #4).
    // Returns null (not an error) when the score doesn't clear the
    // threshold — "no match created" is a normal, expected outcome.
    Task<UserJobMatchDto?> TryCreateMatchAsync(
        Guid userId,
        Guid jobId,
        int score,
        string reason,
        CancellationToken ct = default);

    // Worker agent loop's send_digest_email tool reads this to build the
    // email without the model enumerating matches itself.
    Task<List<UserJobMatchDto>> GetUnnotifiedAsync(
        Guid userId,
        CancellationToken ct = default);

    Task MarkAsNotifiedAsync(
        Guid userId,
        List<Guid> jobIds,
        CancellationToken ct = default);
}
