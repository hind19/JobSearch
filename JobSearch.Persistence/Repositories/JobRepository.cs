using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobPersistenceDto?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var entity = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }

    public async Task<JobPersistenceDto?> GetByUrlHashAsync(
        string urlHash,
        CancellationToken ct = default)
    {
        var entity = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                j => j.UrlHash == urlHash, ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }

    public async Task<bool> ExistsByUrlHashAsync(
        string urlHash,
        CancellationToken ct = default) =>
        await _context.Jobs
            .AnyAsync(j => j.UrlHash == urlHash, ct);

    public async Task<JobPersistenceDto> CreateAsync(
        JobPersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(dto);

        await _context.Jobs.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<List<JobPersistenceDto>> CreateRangeAsync(
        List<JobPersistenceDto> jobs,
        CancellationToken ct = default)
    {
        var entities = jobs
            .Select(PersistenceMapper.ToEntity)
            .ToList();

        await _context.Jobs.AddRangeAsync(entities, ct);
        await _context.SaveChangesAsync(ct);

        return entities
            .Select(PersistenceMapper.ToDto)
            .ToList();
    }
}