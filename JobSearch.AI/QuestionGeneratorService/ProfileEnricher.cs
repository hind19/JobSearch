// JobSearch.AI/Services/ProfileEnricher.cs
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;

namespace JobSearch.AI.Services;

internal sealed class ProfileEnricher : IProfileEnricher
{
    private readonly AnthropicClient _client;
    private readonly string _model;

    public ProfileEnricher(AnthropicClient client, IOptions<AnthropicSettings> anthropicSettings)
    {
        _client = client;
        _model = anthropicSettings.Value.Models.ProfileEnricher;
    }

    public async Task<string> EnrichAsync(
        string claudeReadyProfile,
        List<ClarifyingQuestionDto> answers,
        CancellationToken ct = default)
    {
        var answersBlock = BuildAnswersBlock(answers);

        var prompt = $"""
            Below is a structured candidate profile extracted from a CV.
            The candidate has also answered several clarifying questions.
            Enrich the profile by incorporating the answers naturally into the existing text.
            Do not repeat information already present. Keep the profile concise and structured.

            ## Current profile:
            {claudeReadyProfile}

            ## Clarifying question answers:
            {answersBlock}

            Return only the updated profile text, no preamble.
            """;

        var request = new MessageParameters
        {
            Model = _model,
            MaxTokens = 1024,
            Messages =
            [
                new Message
                {
                    Role = RoleType.User,
                    Content = [new TextContent { Text = prompt }]
                }
            ]
        };

        var response = await _client.Messages.GetClaudeMessageAsync(request, ct);

        return response.Content
            .OfType<TextContent>()
            .FirstOrDefault()
            ?.Text ?? claudeReadyProfile; // fallback — не теряем оригинал
    }

    private static string BuildAnswersBlock(List<ClarifyingQuestionDto> answers)
    {
        var sb = new StringBuilder();
        foreach (var q in answers)
        {
            sb.AppendLine($"Q: {q.QuestionText}");

            var answer = q.AnswerType switch
            {
                _ when q.TextAnswer is not null => q.TextAnswer,
                _ when q.SelectedAnswer is not null => q.SelectedAnswer,
                _ when q.RangeFrom is not null || q.RangeTo is not null =>
                    $"{q.RangeFrom}–{q.RangeTo} {q.Currency}",
                _ => "(no answer)"
            };

            sb.AppendLine($"A: {answer}");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}