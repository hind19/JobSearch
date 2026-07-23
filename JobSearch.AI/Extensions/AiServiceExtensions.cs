using Anthropic.SDK;
using JobSearch.AI.CvParserService;
using JobSearch.AI.JobSearchAgentService;
using JobSearch.AI.JobSearchAgentService.Tools;
using JobSearch.AI.QuestionGeneratorService;
using JobSearch.AI.Services;
using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.Interfaces;
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
            new AnthropicClient(new APIAuthentication(apiKey)));

        // Bound here (not in BusinessServiceExtensions) so JobMatchService
        // (JobSearch.Business) can read RelevanceThreshold via
        // IOptions<AnthropicSettings> without Business taking a package
        // dependency on JobSearch.AI. AddBusinessServices() has no
        // IConfiguration parameter to bind it there instead.
        services.Configure<AnthropicSettings>(
            configuration.GetSection("AnthropicSettings"));

        services.AddScoped<ICvParser, CvParser>();
        services.AddScoped<IQuestionGenerator, QuestionGenerator>();
        services.AddScoped<IProfileEnricher, ProfileEnricher>();
        services.AddScoped<ISelectorDetector, SelectorDetector>();

        // ADR-0004: Worker's agent loop. JobSearchAgentRunContext is
        // Scoped so all tools + the orchestrator share one instance per
        // run (see Program.cs: one DI scope per WorkerRun invocation).
        services.AddScoped<JobSearchAgentRunContext>();
        services.AddSingleton<IHtmlCleaner, HtmlCleaner>();

        services.AddScoped<IAgentTool, ScrapeJobLinksTool>();
        services.AddScoped<IAgentTool, CheckJobExistsTool>();
        services.AddScoped<IAgentTool, FetchJobPageTool>();
        services.AddScoped<IAgentTool, SaveJobTool>();
        services.AddScoped<IAgentTool, ScoreRelevanceTool>();
        // NOTE: send_digest_email is not registered as a tool — blocked
        // on IEmailSender, which is intentionally deferred. WorkerRun
        // should call IJobMatchService.GetUnnotifiedAsync itself after
        // the agent loop finishes and log a pending-notification warning
        // until that's implemented.

        services.AddScoped<IJobSearchAgent, JobSearchAgent>();

        return services;
    }
}