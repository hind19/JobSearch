// JobSearch.Application.Abstractions/Configuration/EmailSettingsSeedOptions.cs
namespace JobSearch.Application.Abstractions.Configuration;

// Bound from appsettings.json's "EmailSettings" section. Used only once
// per deployment — to seed the EmailSettings DB table if it's empty
// (ADR-0005 §1), not read at send time afterwards. No password field:
// that's read directly from configuration at send time instead (see
// EmailSender), never through this options class.
public class EmailSettingsSeedOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = string.Empty;
}
