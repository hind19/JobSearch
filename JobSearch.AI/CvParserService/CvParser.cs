using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using JobSearch.AI.Mapping;
using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace JobSearch.AI.CvParserService;

public class CvParser : ICvParser
{
    private const int MaxTokens = 8192;

    private readonly AnthropicClient _client;
    private readonly string _model;
    private readonly ILogger<CvParser> _logger;

    public CvParser(
        AnthropicClient client,
        IOptions<AnthropicSettings> anthropicSettings,
        ILogger<CvParser> logger)
    {
        _client = client;
        _model = anthropicSettings.Value.Models.CvParser;
        _logger = logger;
    }

    public async Task<CvAnalysisResult> ParseCvAsync(
        byte[] pdfBytes,
        CancellationToken ct)
    {
        try
        {
            var base64String = Convert.ToBase64String(pdfBytes);

            var request = new MessageParameters
            {
                Model = _model,
                MaxTokens = MaxTokens,
                System = [new SystemMessage(CvParserPrompts.System)],
                Messages =
                [
                    new Message
                    {
                        Role = RoleType.User,
                        Content =
                        [
                            new DocumentContent
                            {
                                Source = new DocumentSource
                                {
                                    Type = SourceType.base64,
                                    MediaType = "application/pdf",
                                    Data = base64String
                                }
                            },
                            new TextContent
                            {
                                Text = CvParserPrompts.User
                            }
                        ]
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
                return Fail("Claude returned empty response.");
            }

            // Remove markdown wrapper if Claude added it anyway
            json = json.Trim();
            if (json.StartsWith("```"))
            {
                json = json
                    .Replace("```json", string.Empty)
                    .Replace("```", string.Empty)
                    .Trim();
            }

            var raw = JsonSerializer.Deserialize<CvAnalysisRaw>(json);

            return DeserializeAiResponse(json);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CV parsing failed.");
            return Fail($"CV parsing failed: {ex.Message}");
        }
    }

    private CvAnalysisResult DeserializeAiResponse(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var raw = JsonSerializer
                .Deserialize<CvAnalysisRaw>(json, options);

            if (raw is null)
                return Fail("Failed to deserialize Claude response.");

            return CvAnalysisMapper.ToResult(raw);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
                "Failed to deserialize CV analysis response. " +
                "Raw JSON: {Json}", json);

            return Fail("Failed to parse Claude response as JSON.");
        }
    }

    private CvAnalysisResult Fail(string message) => CvAnalysisResult.Failure("Claude returned empty response.");
}