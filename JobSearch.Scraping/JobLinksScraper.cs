// JobSearch.Scraping/JobLinksScraper.cs
using HtmlAgilityPack;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;

namespace JobSearch.Scraping;

public class JobLinksScraper : IJobLinksScraper
{
    private readonly HttpClient _httpClient;

    public JobLinksScraper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<string>> ScrapeLinksAsync(
    JobSiteDto jobSite,
    CancellationToken ct = default)
    {
        var containerSelector = jobSite.ScrapeConfig.ContainerSelector;
        var linkSelector = jobSite.ScrapeConfig.LinkSelector;

        if (string.IsNullOrWhiteSpace(containerSelector) ||
            string.IsNullOrWhiteSpace(linkSelector))
            return [];

        var url = jobSite.ScrapeConfig.SearchUrlTemplate
            .Replace("{query}", Uri.EscapeDataString(
                jobSite.ScrapeConfig.SearchQuery));

        var html = await _httpClient.GetStringAsync(url, ct);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var containers = doc.DocumentNode
            .QuerySelectorAll(containerSelector);

        var links = containers
            .Select(container => container.QuerySelector(linkSelector))
            .Where(a => a is not null)
            .Select(a => a!.GetAttributeValue("href", string.Empty))
            .Where(href => !string.IsNullOrWhiteSpace(href))
            .Select(href => NormalizeUrl(href, jobSite.BaseUrl))
            .Distinct()
            .ToList();

        return links;
    }

    public async Task<string> FetchHtmlAsync(
    string url,
    CancellationToken ct = default) =>
    await _httpClient.GetStringAsync(url, ct);

    private static string NormalizeUrl(string href, string baseUrl)
    {
        if (href.StartsWith("http://") || href.StartsWith("https://"))
            return href;

        var base_ = new Uri(baseUrl);
        return new Uri(base_, href).ToString();
    }
}