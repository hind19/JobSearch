using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class JobSiteRepository : IJobSiteRepository
{
    private readonly AppDbContext _context;

    public JobSiteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobSitePersistenceDto>> GetAllActiveAsync(
        CancellationToken ct = default)
    {
        var entities = await _context.JobSites
            .AsNoTracking()
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        return entities
            .Select(PersistenceMapper.ToDto)
            .ToList();
    }

    public async Task<JobSitePersistenceDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var entity = await _context.JobSites
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }

    public async Task<JobSitePersistenceDto> CreateAsync(
        JobSitePersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(dto);

        await _context.JobSites.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<JobSitePersistenceDto> UpdateAsync(
        JobSitePersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = await _context.JobSites
            .FirstOrDefaultAsync(s => s.Id == dto.Id, ct)
                ?? throw new InvalidOperationException(
                    $"JobSite {dto.Id} not found.");

        entity.Name = dto.Name;
        entity.BaseUrl = dto.BaseUrl;
        entity.IsActive = dto.IsActive;
        entity.ScrapeConfig = dto.ScrapeConfig;

        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken ct = default)
    {
        var entity = await _context.JobSites
            .FirstOrDefaultAsync(s => s.Id == id, ct)
                ?? throw new InvalidOperationException(
                    $"JobSite {id} not found.");

        entity.IsActive = isActive;

        await _context.SaveChangesAsync(ct);
    }
}