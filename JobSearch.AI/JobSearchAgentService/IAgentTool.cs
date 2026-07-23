// JobSearch.AI/JobSearchAgentService/IAgentTool.cs
using System.Text.Json.Nodes;

namespace JobSearch.AI.JobSearchAgentService;

// Internal — the orchestrator (JobSearchAgent) dispatches to these by
// name. Not exposed outside JobSearch.AI; JobSearch.Worker only ever
// talks to IJobSearchAgent.
internal interface IAgentTool
{
    string Name { get; }
    string Description { get; }
    JsonObject InputSchema { get; }

    // Returns a JSON string — becomes the tool_result content sent back
    // to Claude. Never throws for expected/guardrail-rejection cases
    // (returns { "error": "..." } instead) so the loop can continue;
    // only throws for genuine bugs (e.g. JobMatchService.TryCreateMatchAsync's
    // defensive check on an unknown jobId).
    Task<string> ExecuteAsync(Guid userId, JsonNode? input, CancellationToken ct);
}
