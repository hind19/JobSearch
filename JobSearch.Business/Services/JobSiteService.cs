// JobSearch.Business/Services/JobSiteService.cs
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
using JobSearch.Persistence.Abstractions;

namespace JobSearch.Business.Services;

internal sealed class JobSiteService : IJobSiteService
{
    private readonly IJobSiteRepository _jobSiteRepository;
    private readonly IJobLinksScraper _jobLinksScraper;

    public JobSiteService(
        IJobSiteRepository jobSiteRepository,
        IJobLinksScraper jobLinksScraper)
    {
        _jobSiteRepository = jobSiteRepository;
        _jobLinksScraper = jobLinksScraper;
    }

    public async Task<List<JobSiteDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var dtos = await _jobSiteRepository.GetAllAsync(ct);
        return dtos.Select(BusinessMapper.ToDto).ToList();
    }

    // IJobSiteQueryService — used by JobSearch.Worker to load only the
    // sites it should scrape, without depending on the full CRUD surface.
    public async Task<List<JobSiteDto>> GetAllActiveAsync(
        CancellationToken ct = default)
    {
        var dtos = await _jobSiteRepository.GetAllActiveAsync(ct);
        return dtos.Select(BusinessMapper.ToDto).ToList();
    }

    public async Task<JobSiteDto> CreateAsync(
        JobSiteDto dto,
        CancellationToken ct = default)
    {
        var persistenceDto = BusinessMapper.ToPersistenceDto(dto);
        var created = await _jobSiteRepository.CreateAsync(persistenceDto, ct);
        return BusinessMapper.ToDto(created);
    }

    public async Task<JobSiteDto> UpdateAsync(
        JobSiteDto dto,
        CancellationToken ct = default)
    {
        var persistenceDto = BusinessMapper.ToPersistenceDto(dto);
        var updated = await _jobSiteRepository.UpdateAsync(persistenceDto, ct);
        return BusinessMapper.ToDto(updated);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken ct = default) =>
        await _jobSiteRepository.DeleteAsync(id, ct);

    public async Task SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken ct = default) =>
        await _jobSiteRepository.SetActiveAsync(id, isActive, ct);

    public async Task<(bool IsValid, string? ErrorMessage, List<string> Links)> ValidateConfigAsync(
     JobSiteDto dto,
     CancellationToken ct = default)
    {
        try
        {
            var links = await _jobLinksScraper.ScrapeLinksAsync(dto, ct);

            if (links.Count == 0)
                return (false, null, []);

            return (true, null, links);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, []);
        }
    }
}