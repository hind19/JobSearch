// JobSearch.Application.Abstractions/Interfaces/ISelectorDetector.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

public interface ISelectorDetector
{
    Task<ScrapeConfigDto> DetectFromHtmlAsync(
        string html,
        CancellationToken ct = default);

    Task<ScrapeConfigDto> DetectFromUrlAsync(
        string url,
        CancellationToken ct = default);
}