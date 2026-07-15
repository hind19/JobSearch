// JobSearch.Application.Abstractions/Interfaces/IJobLinksScraper.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

public interface IJobLinksScraper
{
    Task<List<string>> ScrapeLinksAsync(
        JobSiteDto jobSite,
        CancellationToken ct = default);

    Task<string> FetchHtmlAsync(string url, CancellationToken ct = default);
}