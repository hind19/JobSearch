// JobSearch.Business/Services/JobService.cs
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
using JobSearch.Persistence.Abstractions;

namespace JobSearch.Business.Services;

internal sealed class JobService : IJobService
{
    private readonly IUserJobMatchRepository _userJobMatchRepository;

    public JobService(IUserJobMatchRepository userJobMatchRepository)
    {
        _userJobMatchRepository = userJobMatchRepository;
    }

    public async Task<List<UserJobMatchDto>> GetMatchesByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var dtos = await _userJobMatchRepository.GetByUserIdAsync(userId, ct);
        return BusinessMapper.ToDto(dtos);
    }
}
