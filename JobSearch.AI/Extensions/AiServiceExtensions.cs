using Anthropic.SDK;
using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JobSearch.AI.CvParserService;
using JobSearch.AI.QuestionGeneratorService;

namespace JobSearch.AI;

public static class AiServiceExtensions
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["AnthropicSettings:ApiKey"]
             ?? throw new InvalidOperationException(
                 "Anthropic API key not found. " +
                 "Configure via user-secrets or environment variable " +
                 "'AnthropicSettings__ApiKey'.");

        services.AddSingleton<AnthropicClient>(_ =>
            new AnthropicClient(new APIAuthentication(apiKey)));

        services.AddScoped<ICvParser, CvParser>();
        services.AddScoped<IQuestionGenerator, QuestionGenerator>();

        return services;
    }
}