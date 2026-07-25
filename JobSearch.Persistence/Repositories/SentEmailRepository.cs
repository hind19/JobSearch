using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class SentEmailRepository : ISentEmailRepository
{
    private readonly AppDbContext _context;

    public SentEmailRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SentEmailPersistenceDto> CreateAsync(
        SentEmailPersistenceDto sentEmail,
        CancellationToken ct = default)
    {
        var entity = PersistenceMapper.ToEntity(sentEmail);

        await _context.SentEmails.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(entity);
    }

    public async Task UpdateStatusAsync(
        Guid id,
        string status,
        int attemptCount,
        string? errorMessage,
        DateTime? sentAt,
        CancellationToken ct = default)
    {
        var entity = await _context.SentEmails
            .FirstOrDefaultAsync(e => e.Id == id, ct)
                ?? throw new InvalidOperationException(
                    $"SentEmail {id} not found.");

        entity.Status = status;
        entity.AttemptCount = attemptCount;
        entity.ErrorMessage = errorMessage;
        entity.SentAt = sentAt;

        await _context.SaveChangesAsync(ct);
    }
}
