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
    private readonly IJobRepository _jobRepository;
    private readonly IOptions<AnthropicSettings> _anthropicSettings;

    public JobMatchService(
        IUserJobMatchRepository userJobMatchRepository,
        IJobRepository jobRepository,
        IOptions<AnthropicSettings> anthropicSettings)
    {
        _userJobMatchRepository = userJobMatchRepository;
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

        if (clampedScore < threshold)
            return null; // not an error — "below threshold" is a normal outcome

        var job = await _jobRepository.GetByIdAsync(jobId, ct)
            ?? throw new InvalidOperationException(
                $"Job {jobId} not found — score_relevance must be called " +
                "with a jobId returned by a prior save_job call in this run.");

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
