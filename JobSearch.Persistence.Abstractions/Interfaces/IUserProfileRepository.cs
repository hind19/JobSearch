// Interfaces/IUserProfileRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface IUserProfileRepository
{
    Task<UserProfilePersistenceDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<UserProfilePersistenceDto> CreateAsync(
        UserProfilePersistenceDto profile,
        CancellationToken ct = default);

    Task<UserProfilePersistenceDto> UpdateAsync(
        UserProfilePersistenceDto profile,
        CancellationToken ct = default);

    Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<string?> GetClaudeReadyProfileAsync(
        Guid userId,
        CancellationToken ct = default);
}