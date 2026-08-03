// JobSearch.AI/Services/SelectorDetector.cs
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using JobSearch.Application.Abstractions.Configuration;
using Microsoft.Extensions.Options;

namespace JobSearch.AI.Services;

public sealed class SelectorDetector : ISelectorDetector
{
    private const int MaxTokens = 1024;

    private readonly AnthropicClient _client;
    private readonly string _model;
    private readonly IJobLinksScraper _jobLinksScraper;
    private readonly ILogger<SelectorDetector> _logger;

    public SelectorDetector(
        AnthropicClient client,
        IOptions<AnthropicSettings> anthropicSettings,
        IJobLinksScraper jobLinksScraper,
        ILogger<SelectorDetector> logger)
    {
        _client = client;
        _model = anthropicSettings.Value.Models.SelectorDetector;
        _jobLinksScraper = jobLinksScraper;
        _logger = logger;
    }

    public async Task<ScrapeConfigDto> DetectFromHtmlAsync(
        string html,
        CancellationToken ct = default)
    {
        return await AnalyzeHtmlAsync(html, ct);
    }

    public async Task<ScrapeConfigDto> DetectFromUrlAsync(
        string url,
        CancellationToken ct = default)
    {
        var html = await _jobLinksScraper.FetchHtmlAsync(url, ct);
        return await AnalyzeHtmlAsync(html, ct);
    }

    private async Task<ScrapeConfigDto> AnalyzeHtmlAsync(
        string html,
        CancellationToken ct)
    {
        var prompt = BuildPrompt(html);

        var request = new MessageParameters
        {
            Model = _model,
            MaxTokens = MaxTokens,
            Messages =
            [
                new Message
                {
                    Role = RoleType.User,
                    Content = [new TextContent { Text = prompt }]
                }
            ]
        };

        try
        {
            var response = await _client.Messages
                .GetClaudeMessageAsync(request, ct);

            var json = response.Content
                .OfType<TextContent>()
                .FirstOrDefault()
                ?.Text ?? string.Empty;

            json = json.Trim();
            if (json.StartsWith("```"))
            {
                json = json
                    .Replace("```json", string.Empty)
                    .Replace("```", string.Empty)
                    .Trim();
            }

            return ParseResponse(json);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Selector detection failed.");
            return ScrapeConfigDto.Empty;
        }
    }

    private ScrapeConfigDto ParseResponse(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var raw = JsonSerializer
                .Deserialize<SelectorDetectorRaw>(json, options);

            if (raw is null)
                return ScrapeConfigDto.Empty;

            return new ScrapeConfigDto(
                 searchUrlTemplate: string.Empty,
                 searchQuery: string.Empty,
                 containerSelector: raw.ContainerSelector,
                 linkSelector: raw.LinkSelector,
                 companySelector: raw.CompanySelector,
                 snippetSelector: raw.SnippetSelector,
                 dateSelector: raw.DateSelector
 );
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
                "Failed to deserialize selector detection response. " +
                "Raw JSON: {Json}", json);

            return ScrapeConfigDto.Empty;
        }
    }

    private static string BuildPrompt(string html)
    {
        const string schema = """
        {
            "containerSelector": "string or null",
            "linkSelector": "string or null",
            "companySelector": "string or null",
            "snippetSelector": "string or null",
            "dateSelector": "string or null"
        }
        """;

        return
            $$"""
        IMPORTANT: Return ONLY raw JSON. No markdown, no ```json fences, no backticks, no explanation. The very first character must be '{'.

        You are an HTML analysis expert. Analyze the provided HTML of a job listings page and identify the CSS selectors for the following elements.

        Rules:
        - containerSelector: CSS selector for a single job listing container <div>. All other selectors are relative to this container.
        - linkSelector: CSS selector for the <a> tag with the job URL inside the container.
        - companySelector: CSS selector for the company name element inside the container.
        - snippetSelector: CSS selector for the job description snippet inside the container.
        - dateSelector: CSS selector for the publication date element inside the container.
        - Prefer class-based selectors over tag-only selectors for reliability.
        - If an element cannot be reliably identified, return null for that field.

        Return ONLY a valid JSON object following this schema:
        {{schema}}

        HTML to analyze:
        {{html}}
        """;
    }
}