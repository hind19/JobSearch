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

    public AnthropicModelSettings Models { get; set; } = new();
}

// One model per AI service, tuned independently by effectiveness/cost:
// CvParser/SelectorDetector → Sonnet, QuestionGenerator/ProfileEnricher
// → Haiku, JobSearchAgent (unattended Worker loop, ADR-0004) → Opus.
public class AnthropicModelSettings
{
    public string CvParser { get; set; } = string.Empty;
    public string QuestionGenerator { get; set; } = string.Empty;
    public string ProfileEnricher { get; set; } = string.Empty;
    public string SelectorDetector { get; set; } = string.Empty;
    public string JobSearchAgent { get; set; } = string.Empty;
}
