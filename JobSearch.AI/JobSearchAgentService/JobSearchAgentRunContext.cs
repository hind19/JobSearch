// JobSearch.AI/JobSearchAgentService/JobSearchAgentRunContext.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.AI.JobSearchAgentService;

// Scoped — one instance per DI scope, i.e. one instance per WorkerRun
// invocation (see Program.cs: "one scope for the whole run"). Shared by
// every IAgentTool so guardrails hold across the whole conversation, not
// just within a single tool call.
internal sealed class JobSearchAgentRunContext
{
    // ADR-0004 guardrail #2: url -> the site it was actually returned
    // from by scrape_job_links. fetch_job_page and save_job both check
    // this before acting — a url Claude didn't get from a real tool call
    // in this run is rejected.
    private readonly Dictionary<string, Guid> _knownUrlToSiteId = new();

    public Dictionary<Guid, JobSiteDto> ActiveSitesById { get; } = new();

    public int ToolCallCount { get; private set; }
    public int JobsSaved { get; private set; }
    public int MatchesCreated { get; private set; }

    public void RegisterKnownUrl(string url, Guid jobSiteId) =>
        _knownUrlToSiteId[url] = jobSiteId;

    public bool TryGetSiteIdForUrl(string url, out Guid siteId) =>
        _knownUrlToSiteId.TryGetValue(url, out siteId);

    public void IncrementToolCallCount() => ToolCallCount++;
    public void IncrementJobsSaved() => JobsSaved++;
    public void IncrementMatchesCreated() => MatchesCreated++;
}
