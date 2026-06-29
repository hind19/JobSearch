// Interfaces/IJobSiteRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface IJobSiteRepository
{
    Task<List<JobSitePersistenceDto>> GetAllActiveAsync(
        CancellationToken ct = default);

    Task<JobSitePersistenceDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<JobSitePersistenceDto> CreateAsync(
        JobSitePersistenceDto jobSite,
        CancellationToken ct = default);

    Task<JobSitePersistenceDto> UpdateAsync(
        JobSitePersistenceDto jobSite,
        CancellationToken ct = default);

    Task SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken ct = default);
}