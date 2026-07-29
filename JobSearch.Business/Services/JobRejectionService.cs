// JobSearch.Business/Services/JobRejectionService.cs — ADR-0009
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
using JobSearch.Persistence.Abstractions;

namespace JobSearch.Business.Services;

internal sealed class JobRejectionService : IJobRejectionService
{
    private readonly IUserJobRejectionRepository _repository;

    public JobRejectionService(IUserJobRejectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<DateTime?> GetMostRecentAnalysisDateAsync(
        Guid userId,
        CancellationToken ct = default) =>
        await _repository.GetMostRecentAnalyzedDateAsync(userId, ct);

    public async Task<RejectedJobsPageDto> GetRejectedJobsAsync(
        Guid userId,
        DateTime date,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var pageResult = await _repository.GetByUserIdAndDateAsync(
            userId, date, page, pageSize, ct);

        return BusinessMapper.ToDto(pageResult, page, pageSize);
    }
}
