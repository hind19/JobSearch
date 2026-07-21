using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserPersistenceDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }

    public async Task<UserPersistenceDto?> GetByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Email == email, ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }

    public async Task<UserPersistenceDto> CreateAsync(
        UserPersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(dto);

        // ADR-0002: UpdatedAt is stamped by the repository, not passed
        // through the DTO — write time is a persistence-layer concern.
        entity.UpdatedAt = entity.CreatedAt;

        await _context.Users.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<UserPersistenceDto> UpdateAsync(
        UserPersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.Id, ct)
                ?? throw new InvalidOperationException(
                    $"User {dto.Id} not found.");

        entity.Email = dto.Email;
        entity.Name = dto.Name;
        entity.IsActive = dto.IsActive;
        // ADR-0002: bump UpdatedAt on every write so Worker's
        // "most recently modified user" resolution stays accurate.
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken ct = default) =>
        await _context.Users
            .AnyAsync(u => u.Id == id, ct);

    public async Task<UserPersistenceDto?> GetMostRecentlyModifiedAsync(
        CancellationToken ct = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }
}