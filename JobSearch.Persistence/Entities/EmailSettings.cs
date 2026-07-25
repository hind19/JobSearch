// Entities/EmailSettings.cs
namespace JobSearch.Persistence.Entities;

// ADR-0005: single-row table, no password/credential property by design.
public class EmailSettings
{
    public Guid Id { get; set; }
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
