// JobSearch.Business/Services/EmailSettingsService.cs
using JobSearch.Application.Abstractions.Configuration;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Interfaces;
using JobSearch.Business.Mapping;
using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Microsoft.Extensions.Options;

namespace JobSearch.Business.Services;

internal sealed class EmailSettingsService : IEmailSettingsService
{
    private readonly IEmailSettingsRepository _repository;
    private readonly IOptions<EmailSettingsSeedOptions> _seedOptions;

    public EmailSettingsService(
        IEmailSettingsRepository repository,
        IOptions<EmailSettingsSeedOptions> seedOptions)
    {
        _repository = repository;
        _seedOptions = seedOptions;
    }

    public async Task<EmailSettingsDto?> GetAsync(
        CancellationToken ct = default)
    {
        var existing = await _repository.GetAsync(ct);
        if (existing is not null)
            return BusinessMapper.ToDto(existing);

        // ADR-0005 §1: seed the DB row from appsettings.json on first
        // access if the table is empty.
        var seed = _seedOptions.Value;

        if (string.IsNullOrWhiteSpace(seed.SmtpHost))
            return null; // nothing configured anywhere — WPF form starts blank

        var seeded = new EmailSettingsPersistenceDto(
            id: Guid.NewGuid(),
            smtpHost: seed.SmtpHost,
            smtpPort: seed.SmtpPort,
            useSsl: seed.UseSsl,
            smtpUsername: seed.SmtpUsername,
            fromAddress: seed.FromAddress,
            fromDisplayName: seed.FromDisplayName,
            updatedAt: DateTime.UtcNow);

        var created = await _repository.UpsertAsync(seeded, ct);
        return BusinessMapper.ToDto(created);
    }

    public async Task<EmailSettingsDto> SaveAsync(
        EmailSettingsDto settings,
        CancellationToken ct = default)
    {
        // UpdatedAt is always stamped here, not taken from the caller —
        // same "write time is a persistence-adjacent concern, not the
        // caller's job" reasoning already used for Users.UpdatedAt.
        var normalized = new EmailSettingsDto(
            id: settings.Id,
            smtpHost: settings.SmtpHost,
            smtpPort: settings.SmtpPort,
            useSsl: settings.UseSsl,
            smtpUsername: settings.SmtpUsername,
            fromAddress: settings.FromAddress,
            fromDisplayName: settings.FromDisplayName,
            updatedAt: DateTime.UtcNow);

        var persistenceDto = BusinessMapper.ToPersistenceDto(normalized);
        var saved = await _repository.UpsertAsync(persistenceDto, ct);

        return BusinessMapper.ToDto(saved);
    }
}
