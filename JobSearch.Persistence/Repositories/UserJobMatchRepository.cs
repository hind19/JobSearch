using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class UserJobMatchRepository : IUserJobMatchRepository
{
    private readonly AppDbContext _context;

    public UserJobMatchRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid jobId,
        CancellationToken ct = default) =>
        await _context.UserJobMatches
            .AnyAsync(
                m => m.UserId == userId &&
                     m.JobId == jobId, ct);

    public async Task<List<UserJobMatchPersistenceDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var entities = await _context.UserJobMatches
            .AsNoTracking()
            .Include(m => m.Job)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.RelevanceScore)
            .ToListAsync(ct);

        return entities
            .Select(PersistenceMapper.ToDto)
            .ToList();
    }

    public async Task<List<UserJobMatchPersistenceDto>>
        GetUnnotifiedByUserIdAsync(
            Guid userId,
            CancellationToken ct = default)
    {
        var entities = await _context.UserJobMatches
            .AsNoTracking()
            .Include(m => m.Job)
            .Where(m => m.UserId == userId &&
                        m.WasNotified == false)
            .OrderByDescending(m => m.RelevanceScore)
            .ToListAsync(ct);

        return entities
            .Select(PersistenceMapper.ToDto)
            .ToList();
    }

    public async Task<UserJobMatchPersistenceDto> CreateAsync(
        UserJobMatchPersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(dto);

        await _context.UserJobMatches.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        var created = await _context.UserJobMatches
            .AsNoTracking()
            .Include(m => m.Job)
            .FirstAsync(m => m.Id == entity.Id, ct);

        return PersistenceMapper.ToDto(created);
    }

    public async Task MarkAsNotifiedAsync(
        Guid userId,
        List<Guid> jobIds,
        CancellationToken ct = default)
    {
        var entities = await _context.UserJobMatches
            .Where(m => m.UserId == userId &&
                        jobIds.Contains(m.JobId))
            .ToListAsync(ct);

        var now = DateTime.UtcNow;

        foreach (var entity in entities)
        {
            entity.WasNotified = true;
            entity.NotifiedAt = now;
        }

        await _context.SaveChangesAsync(ct);
    }
}