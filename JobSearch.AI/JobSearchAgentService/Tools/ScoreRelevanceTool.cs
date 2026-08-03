// JobSearch.AI/JobSearchAgentService/Tools/ScoreRelevanceTool.cs
using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JobSearch.AI.JobSearchAgentService.Tools;

internal sealed class ScoreRelevanceTool : IAgentTool
{
    private readonly IJobMatchService _jobMatchService;
    private readonly IOptions<AnthropicSettings> _settings;
    private readonly JobSearchAgentRunContext _context;

    public ScoreRelevanceTool(
        IJobMatchService jobMatchService,
        IOptions<AnthropicSettings> settings,
        JobSearchAgentRunContext context)
    {
        _jobMatchService = jobMatchService;
        _settings = settings;
        _context = context;
    }

    public string Name => "score_relevance";

    public string Description =>
        """
         Submit your relevance assessment of a saved job against the
         candidate's profile. You compute the score and reasoning
         yourself; this tool just records it.
        """;

    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["jobId"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" },
            ["score"] = new JsonObject
            {
                ["type"] = "integer",
                ["minimum"] = 0,
                ["maximum"] = 100
            },
            ["reason"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "One or two sentences."
            }
        },
        ["required"] = new JsonArray("jobId", "score", "reason")
    };

    public async Task<string> ExecuteAsync(
        Guid userId, JsonNode? input, CancellationToken ct)
    {
        var jobId = Guid.Parse(input!["jobId"]!.GetValue<string>());
        var score = input["score"]!.GetValue<int>();
        var reason = input["reason"]!.GetValue<string>();

        // ADR-0004 guardrail #4: threshold comparison happens inside
        // TryCreateMatchAsync, in C#, on a clamped numeric value.
        var match = await _jobMatchService
            .TryCreateMatchAsync(userId, jobId, score, reason, ct);

        if (match is not null)
            _context.IncrementMatchesCreated();

        return JsonSerializer.Serialize(new
        {
            matched = match is not null,
            thresholdApplied = _settings.Value.RelevanceThreshold
        });
    }
}
