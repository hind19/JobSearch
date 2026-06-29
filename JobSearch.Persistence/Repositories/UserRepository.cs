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

        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken ct = default) =>
        await _context.Users
            .AnyAsync(u => u.Id == id, ct);
}