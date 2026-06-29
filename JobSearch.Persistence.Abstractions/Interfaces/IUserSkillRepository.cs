// Interfaces/IUserSkillRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface IUserSkillRepository
{
    Task<List<UserSkillPersistenceDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<UserSkillPersistenceDto> CreateAsync(
        UserSkillPersistenceDto skill,
        CancellationToken ct = default);

    Task<UserSkillPersistenceDto> UpdateAsync(
        UserSkillPersistenceDto skill,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken ct = default);

    Task DeleteAllByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<List<UserSkillPersistenceDto>> CreateRangeAsync(
        List<UserSkillPersistenceDto> skills,
        CancellationToken ct = default);
}