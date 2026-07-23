// JobSearch.AI/JobSearchAgentService/Tools/SaveJobTool.cs
using System.Text.Json;
using System.Text.Json.Nodes;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;

namespace JobSearch.AI.JobSearchAgentService.Tools;

internal sealed class SaveJobTool : IAgentTool
{
    private readonly IJobIngestService _jobIngestService;
    private readonly JobSearchAgentRunContext _context;

    public SaveJobTool(
        IJobIngestService jobIngestService,
        JobSearchAgentRunContext context)
    {
        _jobIngestService = jobIngestService;
        _context = context;
    }

    public string Name => "save_job";

    public string Description =>
        "Persist a job posting you've extracted from a fetched page. " +
        "Only call this for a URL you actually fetched with " +
        "fetch_job_page in this conversation.";

    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["url"] = new JsonObject { ["type"] = "string" },
            ["title"] = new JsonObject { ["type"] = "string" },
            ["company"] = new JsonObject { ["type"] = "string" },
            ["location"] = new JsonObject { ["type"] = "string" },
            ["salaryRaw"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Salary as stated on the page, unparsed. Empty string if not mentioned."
            },
            ["descriptionRaw"] = new JsonObject { ["type"] = "string" },
            ["postedAt"] = new JsonObject
            {
                ["type"] = "string",
                ["format"] = "date",
                ["description"] = "Omit the field entirely if the posting date isn't stated."
            }
        },
        ["required"] = new JsonArray("url", "title", "company", "descriptionRaw")
    };

    public async Task<string> ExecuteAsync(
        Guid userId, JsonNode? input, CancellationToken ct)
    {
        var url = input!["url"]!.GetValue<string>();

        // ADR-0004 guardrail #2: reject a URL Claude didn't actually
        // fetch via a real tool call in this run.
        if (!_context.TryGetSiteIdForUrl(url, out var jobSiteId))
            return JsonSerializer.Serialize(new
            {
                error = "URL was not returned by scrape_job_links in this run."
            });

        DateTime? postedAt = null;
        if (input["postedAt"] is JsonNode postedAtNode &&
            DateTime.TryParse(postedAtNode.GetValue<string>(), out var parsed))
            postedAt = parsed;

        var job = new JobDto(
            id: Guid.NewGuid(),
            jobSiteId: jobSiteId,
            externalId: null,
            url: url,
            title: input["title"]!.GetValue<string>(),
            company: input["company"]!.GetValue<string>(),
            location: input["location"]?.GetValue<string>() ?? string.Empty,
            salaryRaw: input["salaryRaw"]?.GetValue<string>() ?? string.Empty,
            descriptionRaw: input["descriptionRaw"]!.GetValue<string>(),
            postedAt: postedAt,
            foundAt: DateTime.UtcNow,
            // ADR-0004 guardrail #2: placeholder — JobIngestService
            // recomputes the real hash from url server-side regardless of
            // what's passed here.
            urlHash: string.Empty);

        var created = await _jobIngestService.CreateAsync(job, ct);
        _context.IncrementJobsSaved();

        return JsonSerializer.Serialize(new { jobId = created.Id, saved = true });
    }
}
