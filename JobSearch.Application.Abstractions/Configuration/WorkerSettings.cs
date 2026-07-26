// JobSearch.Application.Abstractions/Configuration/WorkerSettings.cs
namespace JobSearch.Application.Abstractions.Configuration;

public class WorkerSettings
{
    public string ScheduleTime { get; set; } = string.Empty;
    public int DelayBetweenRequestsMs { get; set; }
    public int MaxPagesPerSite { get; set; }

    // ADR-0004 guardrail #3. Default kept generous enough for a full,
    // non-deduplicated first run across a handful of sites (see the run
    // that hit the old 150 cap: 45 saved jobs alone consumed 135+ calls
    // before a single score_relevance call happened) — subsequent runs
    // dedup via check_job_exists and use far fewer calls.
    public int MaxAgentToolCalls { get; set; } = 450;
}
