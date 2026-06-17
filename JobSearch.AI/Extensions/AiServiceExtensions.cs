using Anthropic.SDK;
using JobSearch.AI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            new AnthropicClient(apiKey));

        services.AddScoped<CvParser>();
        services.AddScoped<QuestionGenerator>();

        return services;
    }
}