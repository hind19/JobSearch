// JobSearch.Persistence.Abstractions/DTOs/EmailSettingsPersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

// ADR-0005: no password/credential field here by design — that stays in
// user-secrets only, never persisted to the database.
public class EmailSettingsPersistenceDto(
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
