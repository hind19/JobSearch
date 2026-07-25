// JobSearch.Application.Abstractions/Interfaces/IEmailSettingsService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// WPF-facing (EmailSettingsViewModel), same layering as IJobSiteService —
// WPF talks to Business, never to Persistence directly.
public interface IEmailSettingsService
{
    Task<EmailSettingsDto?> GetAsync(CancellationToken ct = default);

    Task<EmailSettingsDto> SaveAsync(
        EmailSettingsDto settings,
        CancellationToken ct = default);
}
