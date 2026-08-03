// JobSearch.AI/JobSearchAgentService/Tools/ScrapeJobLinksTool.cs
using JobSearch.Application.Abstractions.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JobSearch.AI.JobSearchAgentService.Tools;

internal sealed class ScrapeJobLinksTool : IAgentTool
{
    private readonly IJobLinksScraper _scraper;
    private readonly JobSearchAgentRunContext _context;

    public ScrapeJobLinksTool(
        IJobLinksScraper scraper,
        JobSearchAgentRunContext context)
    {
        _scraper = scraper;
        _context = context;
    }

    public string Name => "scrape_job_links";

    public string Description =>
        "Get the list of job posting URLs currently listed on a given active job site.";

    public JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["jobSiteId"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" }
        },
        ["required"] = new JsonArray("jobSiteId")
    };

    public async Task<string> ExecuteAsync(
        Guid userId, JsonNode? input, CancellationToken ct)
    {
        var jobSiteId = Guid.Parse(input!["jobSiteId"]!.GetValue<string>());

        if (!_context.ActiveSitesById.TryGetValue(jobSiteId, out var site))
            return JsonSerializer.Serialize(new
            {
                error = $"Unknown or inactive jobSiteId: {jobSiteId}"
            });

        var links = await _scraper.ScrapeLinksAsync(site, ct);

        // ADR-0004 guardrail #2: every link is now allow-listed against
        // the site it actually came from.
        foreach (var link in links)
            _context.RegisterKnownUrl(link, jobSiteId);

        return JsonSerializer.Serialize(new { links });
    }
}
