// Interfaces/IUserJobMatchRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface IUserJobMatchRepository
{
    Task<bool> ExistsAsync(
        Guid userId,
        Guid jobId,
        CancellationToken ct = default);

    Task<List<UserJobMatchPersistenceDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<List<UserJobMatchPersistenceDto>> GetUnnotifiedByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<UserJobMatchPersistenceDto> CreateAsync(
        UserJobMatchPersistenceDto match,
        CancellationToken ct = default);

    Task MarkAsNotifiedAsync(
        Guid userId,
        List<Guid> jobIds,
        CancellationToken ct = default);
}