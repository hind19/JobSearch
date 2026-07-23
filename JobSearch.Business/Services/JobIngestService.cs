// JobSearch.Business/Services/JobIngestService.cs
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
using JobSearch.Persistence.Abstractions;

namespace JobSearch.Business.Services;

internal sealed class JobIngestService : IJobIngestService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobUrlHasher _urlHasher;

    public JobIngestService(
        IJobRepository jobRepository,
        IJobUrlHasher urlHasher)
    {
        _jobRepository = jobRepository;
        _urlHasher = urlHasher;
    }

    public async Task<bool> ExistsByUrlAsync(
        string url,
        CancellationToken ct = default)
    {
        var urlHash = _urlHasher.Compute(url);
        return await _jobRepository.ExistsByUrlHashAsync(urlHash, ct);
    }

    public async Task<JobDto> CreateAsync(
        JobDto job,
        CancellationToken ct = default)
    {
        // ADR-0004 guardrail #2: recompute the hash from the URL rather
        // than trusting whatever was on the incoming DTO — the save_job
        // tool doesn't even provide one, so this is the sole source.
        var urlHash = _urlHasher.Compute(job.Url);

        var normalized = new JobDto(
            id: job.Id,
            jobSiteId: job.JobSiteId,
            externalId: job.ExternalId,
            url: job.Url,
            title: job.Title,
            company: job.Company,
            location: job.Location,
            salaryRaw: job.SalaryRaw,
            descriptionRaw: job.DescriptionRaw,
            postedAt: job.PostedAt,
            foundAt: job.FoundAt,
            urlHash: urlHash);

        var persistenceDto = BusinessMapper.ToPersistenceDto(normalized);
        var created = await _jobRepository.CreateAsync(persistenceDto, ct);

        return BusinessMapper.ToDto(created);
    }
}
