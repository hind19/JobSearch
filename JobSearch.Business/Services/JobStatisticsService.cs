// JobSearch.Business/Services/JobStatisticsService.cs
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
using JobSearch.Persistence.Abstractions;

namespace JobSearch.Business.Services;

internal sealed class JobStatisticsService : IJobStatisticsService
{
    private readonly IJobStatisticsRepository _repository;

    public JobStatisticsService(IJobStatisticsRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<JobSiteStatisticsDto>> GetStatisticsAsync(
        CancellationToken ct = default)
    {
        var dtos = await _repository.GetStatisticsAsync(ct);
        return dtos.Select(BusinessMapper.ToDto).ToList();
    }
}
