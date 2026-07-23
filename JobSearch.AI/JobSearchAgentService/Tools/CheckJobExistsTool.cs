// JobSearch.AI/JobSearchAgentService/Tools/CheckJobExistsTool.cs
using System.Text.Json;
using System.Text.Json.Nodes;
using JobSearch.Application.Abstractions.Interfaces;

namespace JobSearch.AI.JobSearchAgentService.Tools;

internal sealed class CheckJobExistsTool : IAgentTool
{
    private readonly IJobIngestService _jobIngestService;

    public CheckJobExistsTool(IJobIngestService jobIngestService) =>
        _jobIngestService = jobIngestService;

    public string Name => "check_job_exists";

    public string Description =>
        "Check whether a job posting URL has already been saved from a " +
        "previous run, before spending effort fetching and parsing it.";

    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["url"] = new JsonObject { ["type"] = "string" }
        },
        ["required"] = new JsonArray("url")
    };

    public async Task<string> ExecuteAsync(
        Guid userId, JsonNode? input, CancellationToken ct)
    {
        var url = input!["url"]!.GetValue<string>();
        var exists = await _jobIngestService.ExistsByUrlAsync(url, ct);
        return JsonSerializer.Serialize(new { exists });
    }
}
