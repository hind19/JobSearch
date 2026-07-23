// JobSearch.AI/JobSearchAgentService/Tools/FetchJobPageTool.cs
using System.Text.Json;
using System.Text.Json.Nodes;
using JobSearch.Application.Abstractions.Interfaces;

namespace JobSearch.AI.JobSearchAgentService.Tools;

internal sealed class FetchJobPageTool : IAgentTool
{
    private readonly IJobLinksScraper _scraper;
    private readonly IHtmlCleaner _htmlCleaner;
    private readonly JobSearchAgentRunContext _context;

    public FetchJobPageTool(
        IJobLinksScraper scraper,
        IHtmlCleaner htmlCleaner,
        JobSearchAgentRunContext context)
    {
        _scraper = scraper;
        _htmlCleaner = htmlCleaner;
        _context = context;
    }

    public string Name => "fetch_job_page";

    public string Description =>
        "Fetch the raw page content for a single job posting URL, so " +
        "you can read and extract its details yourself.";

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

        // ADR-0004 guardrail #2: only fetch URLs that came from a real
        // scrape_job_links call in this run.
        if (!_context.TryGetSiteIdForUrl(url, out _))
            return JsonSerializer.Serialize(new
            {
                error = "URL was not returned by scrape_job_links in this run."
            });

        var html = await _scraper.FetchHtmlAsync(url, ct);
        var text = _htmlCleaner.StripToReadableText(html);

        return JsonSerializer.Serialize(new { text });
    }
}
