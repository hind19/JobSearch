using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class UserSkillRepository : IUserSkillRepository
{
    private readonly AppDbContext _context;

    public UserSkillRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserSkillPersistenceDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var entities = await _context.UserSkills
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

        return entities
            .Select(PersistenceMapper.ToDto)
            .ToList();
    }

    public async Task<UserSkillPersistenceDto> CreateAsync(
        UserSkillPersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(dto);

        await _context.UserSkills.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task<UserSkillPersistenceDto> UpdateAsync(
        UserSkillPersistenceDto dto,
        CancellationToken ct = default)
    {
        var entity = await _context.UserSkills
            .FirstOrDefaultAsync(s => s.Id == dto.Id, ct)
                ?? throw new InvalidOperationException(
                    $"UserSkill {dto.Id} not found.");

        entity.SkillName = dto.SkillName;
        entity.ProficiencyLevel = dto.ProficiencyLevel;
        entity.YearsOfExperience = dto.YearsOfExperience;

        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var entity = await _context.UserSkills
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entity is not null)
        {
            _context.UserSkills.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAllByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var entities = await _context.UserSkills
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

        if (entities.Count == 0)
            return;

        _context.UserSkills.RemoveRange(entities);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<UserSkillPersistenceDto>> CreateRangeAsync(
        List<UserSkillPersistenceDto> skills,
        CancellationToken ct = default)
    {
        var entities = skills
            .Select(PersistenceMapper.ToEntity)
            .ToList();

        await _context.UserSkills.AddRangeAsync(entities, ct);
        await _context.SaveChangesAsync(ct);

        return entities
            .Select(PersistenceMapper.ToDto)
            .ToList();
    }
}