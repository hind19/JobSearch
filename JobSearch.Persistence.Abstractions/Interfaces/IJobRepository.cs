// Interfaces/IJobRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface IJobRepository
{
    Task<JobPersistenceDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<JobPersistenceDto?> GetByUrlHashAsync(
        string urlHash,
        CancellationToken ct = default);

    Task<bool> ExistsByUrlHashAsync(
        string urlHash,
        CancellationToken ct = default);

    Task<JobPersistenceDto> CreateAsync(
        JobPersistenceDto job,
        CancellationToken ct = default);

    Task<List<JobPersistenceDto>> CreateRangeAsync(
        List<JobPersistenceDto> jobs,
        CancellationToken ct = default);
}