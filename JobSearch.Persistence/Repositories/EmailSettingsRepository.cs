using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using JobSearch.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class EmailSettingsRepository : IEmailSettingsRepository
{
    private readonly AppDbContext _context;

    public EmailSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmailSettingsPersistenceDto?> GetAsync(
        CancellationToken ct = default)
    {
        var entity = await _context.EmailSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return entity is null
            ? null
            : PersistenceMapper.ToDto(entity);
    }

    public async Task<EmailSettingsPersistenceDto> UpsertAsync(
        EmailSettingsPersistenceDto settings,
        CancellationToken ct = default)
    {
        // True singleton: always operate on whichever row already exists,
        // regardless of the Id on the incoming dto — there is only ever
        // meant to be one row in this table (ADR-0003/ADR-0005).
        var existing = await _context.EmailSettings
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            var entity = PersistenceMapper.ToEntity(settings);

            await _context.EmailSettings.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return PersistenceMapper.ToDto(entity);
        }

        existing.SmtpHost = settings.SmtpHost;
        existing.SmtpPort = settings.SmtpPort;
        existing.UseSsl = settings.UseSsl;
        existing.SmtpUsername = settings.SmtpUsername;
        existing.FromAddress = settings.FromAddress;
        existing.FromDisplayName = settings.FromDisplayName;
        existing.UpdatedAt = settings.UpdatedAt;

        await _context.SaveChangesAsync(ct);

        return PersistenceMapper.ToDto(existing);
    }
}
