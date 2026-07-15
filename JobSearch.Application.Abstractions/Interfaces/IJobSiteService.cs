// JobSearch.Application.Abstractions/Interfaces/IJobSiteService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

public interface IJobSiteService
{
    Task<List<JobSiteDto>> GetAllAsync(
        CancellationToken ct = default);

    Task<JobSiteDto> CreateAsync(
        JobSiteDto dto,
        CancellationToken ct = default);

    Task<JobSiteDto> UpdateAsync(
        JobSiteDto dto,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken ct = default);

    Task SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken ct = default);

    Task<(bool IsValid, string? ErrorMessage, List<string> Links)> ValidateConfigAsync(
        JobSiteDto dto,
        CancellationToken ct = default);
}