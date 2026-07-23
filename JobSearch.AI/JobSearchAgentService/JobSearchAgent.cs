// JobSearch.AI/JobSearchAgentService/JobSearchAgent.cs
using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobSearch.AI.JobSearchAgentService;

// internal: nothing outside JobSearch.AI references this concrete class —
// consumers depend on IJobSearchAgent (Application.Abstractions) instead.
// Also required for accessibility consistency: the constructor takes
// internal types (IAgentTool, JobSearchAgentRunContext), which a public
// class/constructor isn't allowed to expose.
internal sealed class JobSearchAgent : IJobSearchAgent
{
    private const string Model = "claude-sonnet-4-6";
    private const int MaxTokens = 4096;

    // ADR-0004 guardrail #3: hard cap, independent of what Claude "thinks"
    // it still needs to do.
    // TODO: move to a config value (e.g. WorkerSettings:MaxAgentToolCalls)
    // once that key exists — open question from worker-agent-tool-design.md.
    private const int MaxToolCalls = 150;

    private readonly AnthropicClient _client;
    private readonly IReadOnlyList<IAgentTool> _tools;
    private readonly JobSearchAgentRunContext _runContext;
    private readonly ILogger<JobSearchAgent> _logger;

    public JobSearchAgent(
        AnthropicClient client,
        IEnumerable<IAgentTool> tools,
        JobSearchAgentRunContext runContext,
        ILogger<JobSearchAgent> logger)
    {
        _client = client;
        _tools = tools.ToList();
        _runContext = runContext;
        _logger = logger;
    }

    public async Task<JobSearchAgentResult> RunAsync(
        Guid userId,
        UserProfileDto profile,
        List<JobSiteDto> activeSites,
        CancellationToken ct = default)
    {
        foreach (var site in activeSites)
            _runContext.ActiveSitesById[site.Id] = site;

        // Anthropic.SDK.Common.Tool is qualified explicitly — "Tool" is
        // ambiguous with Anthropic.SDK.Messaging.Tool otherwise.
        var toolDefinitions = _tools
            .Select(t => new Anthropic.SDK.Common.Tool(
                new Anthropic.SDK.Common.Function(t.Name, t.Description, t.InputSchema)))
            .ToList();

        var messages = new List<Message>
        {
            new()
            {
                Role = RoleType.User,
                Content =
                [
                    new TextContent
                    {
                        Text = JobSearchAgentPrompts.BuildInitialUserMessage(profile, activeSites)
                    }
                ]
            }
        };

        var completed = false;

        while (_runContext.ToolCallCount < MaxToolCalls)
        {
            var request = new MessageParameters
            {
                Model = Model,
                MaxTokens = MaxTokens,
                System = [new SystemMessage(JobSearchAgentPrompts.System)],
                Messages = messages,
                Tools = toolDefinitions
            };

            var response = await _client.Messages.GetClaudeMessageAsync(request, ct);
            var toolUses = response.Content.OfType<ToolUseContent>().ToList();

            if (toolUses.Count == 0)
            {
                // Claude gave a final text answer with no further tool
                // calls — this is the normal, successful end of the run.
                completed = true;
                break;
            }

            messages.Add(new Message
            {
                Role = RoleType.Assistant,
                Content = response.Content
            });

            var resultContents = new List<ContentBase>();
            var hitCap = false;

            foreach (var toolUse in toolUses)
            {
                _runContext.IncrementToolCallCount();

                var tool = _tools.FirstOrDefault(t => t.Name == toolUse.Name);
                string resultJson;

                if (tool is null)
                {
                    resultJson = JsonSerializer.Serialize(new
                    {
                        error = $"Unknown tool: {toolUse.Name}"
                    });
                    _logger.LogWarning("Agent requested unknown tool: {ToolName}", toolUse.Name);
                }
                else
                {
                    // ADR-0004 guardrail #5: log every call, its
                    // arguments, and its result for post-hoc audit.
                    _logger.LogInformation(
                        "Tool call #{Count}: {ToolName} {Input}",
                        _runContext.ToolCallCount, toolUse.Name, toolUse.Input?.ToJsonString());

                    resultJson = await tool.ExecuteAsync(userId, toolUse.Input, ct);

                    _logger.LogInformation(
                        "Tool result: {ToolName} -> {Result}", toolUse.Name, resultJson);
                }

                resultContents.Add(new ToolResultContent
                {
                    ToolUseId = toolUse.Id,
                    Content = [new TextContent { Text = resultJson }]
                });

                if (_runContext.ToolCallCount >= MaxToolCalls)
                {
                    hitCap = true;
                    break;
                }
            }

            messages.Add(new Message
            {
                Role = RoleType.User,
                Content = resultContents
            });

            if (hitCap)
            {
                _logger.LogWarning(
                    "Agent hit the {MaxToolCalls}-tool-call cap; stopping run early without a final answer.",
                    MaxToolCalls);
                break;
            }
        }

        return new JobSearchAgentResult(
            toolCallCount: _runContext.ToolCallCount,
            jobsSaved: _runContext.JobsSaved,
            matchesCreated: _runContext.MatchesCreated,
            completed: completed);
    }
}