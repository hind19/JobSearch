// JobSearch.Business/Services/JobMatchService.cs
using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Microsoft.Extensions.Options;

namespace JobSearch.Business.Services;

internal sealed class JobMatchService : IJobMatchService
{
    private readonly IUserJobMatchRepository _userJobMatchRepository;
    private readonly IUserJobRejectionRepository _userJobRejectionRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IOptions<AnthropicSettings> _anthropicSettings;

    public JobMatchService(
        IUserJobMatchRepository userJobMatchRepository,
        IUserJobRejectionRepository userJobRejectionRepository,
        IJobRepository jobRepository,
        IOptions<AnthropicSettings> anthropicSettings)
    {
        _userJobMatchRepository = userJobMatchRepository;
        _userJobRejectionRepository = userJobRejectionRepository;
        _jobRepository = jobRepository;
        _anthropicSettings = anthropicSettings;
    }

    public async Task<UserJobMatchDto?> TryCreateMatchAsync(
        Guid userId,
        Guid jobId,
        int score,
        string reason,
        CancellationToken ct = default)
    {
        // ADR-0004 guardrail #4: the threshold comparison happens here,
        // in C#, on a clamped numeric value — Claude's own confidence in
        // its "reason" text has no bearing on whether a match is created.
        var clampedScore = Math.Clamp(score, 0, 100);
        var threshold = _anthropicSettings.Value.RelevanceThreshold;

        // ADR-0009: the existence guard now runs for both outcomes — a
        // rejection needs a valid Job to record against, same as a match
        // does. (Previously this guard only ran on the match-creation
        // path; below-threshold returned early without touching the
        // repository at all.)
        var job = await _jobRepository.GetByIdAsync(jobId, ct)
            ?? throw new InvalidOperationException(
                $"Job {jobId} not found — score_relevance must be called " +
                "with a jobId returned by a prior save_job call in this run.");

        if (clampedScore < threshold)
        {
            // ADR-0009: "below threshold" is still a normal outcome — no
            // match is created — but the score/reason are no longer
            // discarded; they're persisted so the user can see why a job
            // was rejected.
            var rejectionDto = new UserJobRejectionPersistenceDto(
                id: Guid.NewGuid(),
                userId: userId,
                jobId: jobId,
                relevanceScore: clampedScore,
                relevanceReason: reason,
                analyzedAt: DateTime.UtcNow,
                job: job);

            await _userJobRejectionRepository.CreateAsync(rejectionDto, ct);
            return null;
        }

        var matchDto = new UserJobMatchPersistenceDto(
            id: Guid.NewGuid(),
            userId: userId,
            jobId: jobId,
            relevanceScore: clampedScore,
            relevanceReason: reason,
            wasNotified: false,
            notifiedAt: null,
            foundInRunAt: DateTime.UtcNow,
            job: job);

        var created = await _userJobMatchRepository.CreateAsync(matchDto, ct);
        return BusinessMapper.ToDto(created);
    }

    public async Task<List<UserJobMatchDto>> GetUnnotifiedAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var dtos = await _userJobMatchRepository
            .GetUnnotifiedByUserIdAsync(userId, ct);

        return BusinessMapper.ToDto(dtos);
    }

    public async Task MarkAsNotifiedAsync(
        Guid userId,
        List<Guid> jobIds,
        CancellationToken ct = default) =>
        await _userJobMatchRepository.MarkAsNotifiedAsync(userId, jobIds, ct);
}
