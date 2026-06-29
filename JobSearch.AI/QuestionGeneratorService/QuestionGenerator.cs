using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;
using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobSearch.AI.QuestionGeneratorService
{
    public class QuestionGenerator : IQuestionGenerator
    {
        private const string Model = "claude-sonnet-4-6";
        private const int MaxTokens = 1000;

        private readonly AnthropicClient _client;
        private readonly ILogger<QuestionGenerator> _logger;

        public QuestionGenerator(
            AnthropicClient client,
            ILogger<QuestionGenerator> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<List<ClarifyingQuestionDto>> GetClarifyingQuestionsAsync(
            CvAnalysisResult cvResult,
            CancellationToken ct = default)
        {
            try
            {
                var userMessage = BuildUserMessage(cvResult);

                var request = new MessageParameters
                {
                    Model = Model,
                    MaxTokens = MaxTokens,
                    System = [new SystemMessage(QuestionGeneratorPrompts.System)],
                    Messages =
                    [
                        new Message
                    {
                        Role = RoleType.User,
                        Content = [new TextContent { Text = userMessage }]
                    }
                    ]
                };

                var response = await _client.Messages
                    .GetClaudeMessageAsync(request, ct);

                var json = response.Content
                    .OfType<TextContent>()
                    .FirstOrDefault()
                    ?.Text;

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning(
                        "QuestionGenerator: Claude returned empty response.");
                    return [];
                }

                return Deserialize(json);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "QuestionGenerator: failed to generate questions.");
                return [];
            }
        }

        private static string BuildUserMessage(CvAnalysisResult cvResult)
        {
            var skills = cvResult.Skills
                .Select(s =>
                    $"{s.SkillName} " +
                    $"({s.ProficiencyLevel}, {s.YearsOfExperience} yrs)")
                .ToList();

            return $"""
            CV Analysis Result:

            Candidate: {cvResult.Candidate.FullName ?? "Unknown"}
            Location: {cvResult.Candidate.Location ?? "not specified"}
            Desired roles: {string.Join(", ", cvResult.DesiredRoles.DefaultIfEmpty("not specified"))}
            Languages detected in CV: {string.Join(", ", cvResult.DetectedLanguages.DefaultIfEmpty("not specified"))}

            Skills ({skills.Count} total):
            {string.Join("\n", skills)}

            Work experience entries: {cvResult.WorkExperience.Count}

            Generate clarifying questions for the missing information only.
            """;
        }

        private List<ClarifyingQuestionDto> Deserialize(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var raw = JsonSerializer
                    .Deserialize<List<ClarifyingQuestionRaw>>(json, options);

                if (raw is null || raw.Count == 0)
                {
                    _logger.LogWarning(
                        "QuestionGenerator: deserialized empty list.");
                    return [];
                }

                return raw.Select(ToDto).ToList();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "QuestionGenerator: failed to deserialize. " +
                    "Raw JSON: {Json}", json);
                return [];
            }
        }

        private static ClarifyingQuestionDto ToDto(
    ClarifyingQuestionRaw raw) =>
    new(
        questionText: raw.QuestionText,
        answerType: ParseAnswerType(raw.AnswerType),
        options: raw.Options,
        selectedAnswer: null,    // заполняется пользователем в UI
        rangeFrom: raw.RangeFrom,
        rangeTo: raw.RangeTo,
        currency: raw.Currency,
        textAnswer: null     // заполняется пользователем в UI
    );

        private static AnswerType ParseAnswerType(string value) =>
            Enum.TryParse<AnswerType>(value, ignoreCase: true, out var result)
                ? result
                : AnswerType.Text;
    }
}
