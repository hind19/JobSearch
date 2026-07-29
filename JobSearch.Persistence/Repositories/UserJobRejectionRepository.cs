// ADR-0009
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class UserJobRejectionRepository : IUserJobRejectionRepository
{
    private readonly AppDbContext _context;

    public UserJobRejectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserJobRejectionPersistenceDto> CreateAsync(
        UserJobRejectionPersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(dto);

        await _context.UserJobRejections.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        var created = await _context.UserJobRejections
            .AsNoTracking()
            .Include(r => r.Job)
            .FirstAsync(r => r.Id == entity.Id, ct);

        return PersistenceMapper.ToDto(created);
    }

    public async Task<RejectedJobsPagePersistenceDto> GetByUserIdAndDateAsync(
        Guid userId,
        DateTime date,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var query = _context.UserJobRejections
            .AsNoTracking()
            .Where(r => r.UserId == userId &&
                        r.AnalyzedAt >= dayStart &&
                        r.AnalyzedAt < dayEnd);

        var totalCount = await query.CountAsync(ct);

        var entities = await query
            .Include(r => r.Job)
            .OrderByDescending(r => r.AnalyzedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities
            .Select(PersistenceMapper.ToDto)
            .ToList();

        return new RejectedJobsPagePersistenceDto(items, totalCount);
    }

    public async Task<DateTime?> GetMostRecentAnalyzedDateAsync(
        Guid userId,
        CancellationToken ct = default) =>
        await _context.UserJobRejections
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.AnalyzedAt)
            .Select(r => (DateTime?)r.AnalyzedAt)
            .FirstOrDefaultAsync(ct);
}
