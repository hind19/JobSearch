using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;

    public UserProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfilePersistenceDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var entity = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.UserId == userId, ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }

    public async Task<UserProfilePersistenceDto> CreateAsync(
        UserProfilePersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(dto);

        await _context.UserProfiles.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<UserProfilePersistenceDto> UpdateAsync(
        UserProfilePersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = await _context.UserProfiles
            .FirstOrDefaultAsync(
                p => p.UserId == dto.UserId, ct)
                    ?? throw new InvalidOperationException(
                        $"UserProfile for user {dto.UserId} not found.");

        entity.ClaudeReadyProfile = dto.ClaudeReadyProfile;
        entity.DesiredRoles = dto.DesiredRoles;
        entity.DesiredSalaryMin = dto.DesiredSalaryMin;
        entity.DesiredSalaryMax = dto.DesiredSalaryMax;
        entity.SalaryCurrency = dto.SalaryCurrency;
        entity.LocationPreference = dto.LocationPreference;
        entity.CvParsedAt = dto.CvParsedAt;
        entity.CvFileHash = dto.CvFileHash;
        entity.UpdatedAt = dto.UpdatedAt;

        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken ct = default) =>
        await _context.UserProfiles
            .AnyAsync(p => p.UserId == userId, ct);

    public async Task<string?> GetClaudeReadyProfileAsync(
        Guid userId,
        CancellationToken ct = default) =>
        await _context.UserProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.ClaudeReadyProfile)
            .FirstOrDefaultAsync(ct);
}