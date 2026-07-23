// JobSearch.Application.Abstractions/Interfaces/IJobIngestService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// Write-side job persistence for JobSearch.Worker's agent loop
// (check_job_exists / save_job tools — see ADR-0004 and
// worker-agent-tool-design.md). Not used by JobSearch.WPF — see
// IJobService for the read-side surface WPF depends on instead.
public interface IJobIngestService
{
    // ADR-0004 guardrail: url is hashed server-side here, never accepted
    // as a pre-computed hash from a caller (and never from tool input —
    // the agent loop only ever passes the raw url).
    Task<bool> ExistsByUrlAsync(
        string url,
        CancellationToken ct = default);

    Task<JobDto> CreateAsync(
        JobDto job,
        CancellationToken ct = default);
}
