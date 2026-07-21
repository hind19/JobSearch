// Interfaces/IUserRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

public interface IUserRepository
{
    Task<UserPersistenceDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<UserPersistenceDto?> GetByEmailAsync(
        string email,
        CancellationToken ct = default);

    Task<UserPersistenceDto> CreateAsync(
        UserPersistenceDto user,
        CancellationToken ct = default);

    Task<UserPersistenceDto> UpdateAsync(
        UserPersistenceDto user,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken ct = default);

    // ADR-0002: Worker resolves the target user via this method instead of
    // login/bypass — returns the user with the most recent UpdatedAt.
    Task<UserPersistenceDto?> GetMostRecentlyModifiedAsync(
        CancellationToken ct = default);
}