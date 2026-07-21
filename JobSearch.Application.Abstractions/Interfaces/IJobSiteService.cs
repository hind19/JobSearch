// JobSearch.Application.Abstractions/Interfaces/IJobSiteService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// Full management surface — used by JobSearch.WPF's JobSitesViewModel
// (list/create/edit/delete/activate/validate). JobSearch.Worker should
// depend on IJobSiteQueryService instead (see that interface) rather than
// on this one, per ISP.
public interface IJobSiteService : IJobSiteQueryService
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