// JobSearch.Application.Abstractions/Interfaces/IJobSearchAgent.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// ADR-0004: agent-loop entry point for JobSearch.Worker. Given a user's
// profile and their active job sites, Claude decides the scrape/parse/
// match sequence itself via tool calls — see worker-agent-tool-design.md.
public interface IJobSearchAgent
{
    Task<JobSearchAgentResult> RunAsync(
        Guid userId,
        UserProfileDto profile,
        List<JobSiteDto> activeSites,
        CancellationToken ct = default);
}
