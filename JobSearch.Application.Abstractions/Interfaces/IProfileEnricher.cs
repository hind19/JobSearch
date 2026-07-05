// JobSearch.Application.Abstractions/Interfaces/IProfileEnricher.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

public interface IProfileEnricher
{
    Task<string> EnrichAsync(
        string claudeReadyProfile,
        List<ClarifyingQuestionDto> answers,
        CancellationToken ct = default);
}