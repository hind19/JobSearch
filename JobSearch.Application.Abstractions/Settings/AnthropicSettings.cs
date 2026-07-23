// JobSearch.Application.Abstractions/Configuration/AnthropicSettings.cs
namespace JobSearch.Application.Abstractions.Configuration;

// Maps the "AnthropicSettings" section of appsettings.json. Lives in
// Application.Abstractions (not JobSearch.AI) so JobSearch.Business can
// depend on RelevanceThreshold (used by JobMatchService.TryCreateMatchAsync,
// ADR-0004 guardrail #4) without taking a dependency on the AI project.
public class AnthropicSettings
{
    public string Model { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public int RelevanceThreshold { get; set; }
}
