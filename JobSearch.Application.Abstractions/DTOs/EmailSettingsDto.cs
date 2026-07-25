// JobSearch.Application.Abstractions/DTOs/EmailSettingsDto.cs
namespace JobSearch.Application.Abstractions.DTOs;

// ADR-0005: no password field — stays in user-secrets, never round-trips
// through this DTO or the WPF form that edits the rest of these fields.
public class EmailSettingsDto(
    Guid id,
    string smtpHost,
    int smtpPort,
    bool useSsl,
    string smtpUsername,
    string fromAddress,
    string fromDisplayName,
    DateTime updatedAt)
{
    public Guid Id { get; } = id;
    public string SmtpHost { get; } = smtpHost;
    public int SmtpPort { get; } = smtpPort;
    public bool UseSsl { get; } = useSsl;
    public string SmtpUsername { get; } = smtpUsername;
    public string FromAddress { get; } = fromAddress;
    public string FromDisplayName { get; } = fromDisplayName;
    public DateTime UpdatedAt { get; } = updatedAt;
}
