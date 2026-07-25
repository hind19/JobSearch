// Interfaces/IEmailSettingsRepository.cs
using JobSearch.Persistence.Abstractions.DTOs;

namespace JobSearch.Persistence.Abstractions;

// Single-row table (ADR-0003: single-user deployment) — no userId/filter
// parameter, unlike the other per-user repositories in this project.
public interface IEmailSettingsRepository
{
    Task<EmailSettingsPersistenceDto?> GetAsync(
        CancellationToken ct = default);

    Task<EmailSettingsPersistenceDto> UpsertAsync(
        EmailSettingsPersistenceDto settings,
        CancellationToken ct = default);
}
