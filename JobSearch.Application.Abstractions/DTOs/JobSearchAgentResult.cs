// JobSearch.Application.Abstractions/DTOs/JobSearchAgentResult.cs
namespace JobSearch.Application.Abstractions.DTOs;

public class JobSearchAgentResult(
    int toolCallCount,
    int jobsSaved,
    int matchesCreated,
    bool completed)
{
    public int ToolCallCount { get; } = toolCallCount;
    public int JobsSaved { get; } = jobsSaved;
    public int MatchesCreated { get; } = matchesCreated;

    // false means the run hit the iteration cap (ADR-0004 guardrail #3)
    // before Claude signaled it was done — WorkerRun should log this as
    // a warning, not treat it as a silent success.
    public bool Completed { get; } = completed;
}
