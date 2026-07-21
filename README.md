# JobSearch

A .NET 9 job-search automation tool with a WPF desktop UI and a background worker service.

## What it does

1. **Parse CV** — upload a PDF; Claude extracts skills, work history, desired roles, and candidate info.
2. **Clarifying questions** — Claude generates follow-up questions (salary range, work format, English level, etc.) that the user answers in the UI.
3. **Save profile** — answers and corrected skills are persisted to a local SQLite database.
4. **Scrape & match** (Worker) — background service scrapes job boards, scores listings against the saved profile, and sends email notifications for strong matches.

## Projects

| Project | Role |
|---|---|
| `JobSearch.WPF` | Desktop UI (WPF, MVVM via CommunityToolkit) |
| `JobSearch.Worker` | Background service / Windows Service |
| `JobSearch.Business` | Application logic, service implementations |
| `JobSearch.AI` | Claude integration — CV parsing and question generation |
| `JobSearch.Scraping` | Playwright browser automation + HtmlAgilityPack |
| `JobSearch.Email` | MailKit email notifications |
| `JobSearch.Persistence` | EF Core 9 + SQLite repositories |
| `JobSearch.Application.Abstractions` | Application-layer interfaces and DTOs |
| `JobSearch.Persistence.Abstractions` | Persistence-layer interfaces and DTOs |

## Prerequisites

- .NET 9 SDK
- An [Anthropic API key](https://console.anthropic.com/)

## Getting started

```powershell
# Restore packages
dotnet restore JobSearch.sln

# Set your Anthropic API key (required at startup)
dotnet user-secrets set "AnthropicSettings:ApiKey" "<your-key>" --project JobSearch.WPF

# Install Playwright browsers (once)
dotnet build JobSearch.Scraping
pwsh JobSearch.Scraping/bin/Debug/net9.0/playwright.ps1 install

# Run the desktop app
dotnet run --project JobSearch.WPF

# Run the background worker
dotnet user-secrets set "AnthropicSettings:ApiKey" "<your-key>" --project JobSearch.Worker
dotnet run --project JobSearch.Worker
```

## Database location

The SQLite database is shared between `JobSearch.WPF` and `JobSearch.Worker` —
both hosts must resolve to the same physical file, so its path is absolute
rather than relative to either project's bin directory:

```
%ProgramData%\JobSearch\Database\jobsearch.db
```

On a default Windows install this is `C:\ProgramData\JobSearch\Database\jobsearch.db`.
The containing directory is created automatically on first run if it doesn't
exist yet (see `PersistenceServiceExtensions.AddPersistence`).

The path is configured via `ConnectionStrings:DefaultConnection` in each
project's `appsettings.json` and supports `%VARIABLE%`-style environment
variable expansion:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=%ProgramData%\\JobSearch\\Database\\jobsearch.db"
}
```

No schema migrations are run against it at startup — the database is
created via `AppDbContext.Database.EnsureCreatedAsync()`. See
`Scripts/CreateDatabase.sql` for the reference schema.
