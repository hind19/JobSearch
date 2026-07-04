# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
# Build entire solution
dotnet build JobSearch.sln

# Build a specific project
dotnet build JobSearch.WPF/JobSearch.WPF.csproj
dotnet build JobSearch.Worker/JobSearch.Worker.csproj

# Run the WPF desktop app
dotnet run --project JobSearch.WPF

# Run the background worker
dotnet run --project JobSearch.Worker

# Restore packages
dotnet restore JobSearch.sln

# EF Core migrations (run from solution root; targets JobSearch.Persistence)
dotnet ef migrations add <MigrationName> --project JobSearch.Persistence --startup-project JobSearch.Worker
dotnet ef database update --project JobSearch.Persistence --startup-project JobSearch.Worker

# Install Playwright browsers (required once after first build)
pwsh JobSearch.Scraping/bin/Debug/net9.0/playwright.ps1 install

# User Secrets (WPF and Worker both support secrets; Anthropic key is required at startup)
dotnet user-secrets set "AnthropicSettings:ApiKey" "<value>" --project JobSearch.WPF
dotnet user-secrets set "AnthropicSettings:ApiKey" "<value>" --project JobSearch.Worker
dotnet user-secrets list --project JobSearch.Worker
```

There are no test projects yet.

## Architecture

This is a .NET 9 job-search automation solution with two entry points: a WPF desktop UI (`JobSearch.WPF`) and a `JobSearch.Worker` background service that can be installed as a Windows Service.

### Stubs (not yet implemented)

- `JobSearch.Business`: `JobService`, `JobMatchService` — empty; `UserProfileService.SaveProfileAsync` — throws `NotImplementedException`
- `JobSearch.Worker`: `Worker.ExecuteAsync` — placeholder loop (1 s delay)
- `UserProfileViewModel`: `Cancel` command is a navigation stub

### Project dependency graph

```
Application.Abstractions ←──────────────────────┐
Persistence.Abstractions ←──────────────────┐   │
                                            │   │
JobSearch.Scraping ←──────────────────── AI ┤   │
                                            │   │
                           Business ────────┼───┘
                                            │
                        Persistence ────────┘
                                            │
                           Email            │
                              │             │
Worker ────────── Business ───┘
       └───────── AI
       └───────── Persistence
       └───────── Email

WPF ──────────── Business
    └─────────── Persistence
    └─────────── Application.Abstractions
```

### Layers

- **`JobSearch.Application.Abstractions`** — Application-layer interfaces: `IJobService`, `IUserProfileService`, `IJobMatchService`, `IEmailSender`, `ICvParser`, `IQuestionGenerator`. DTOs split in two: `CvAnalysisResult`, `CandidateInfo`, `SkillDto`, `WorkExperienceDto` are **declared in the global namespace** (no `namespace` statement); `UserSkillDto` and other typed DTOs are under `JobSearch.Application.Abstractions.DTOs`. `ProficiencyLevel` and `AnswerType` enums live in `Enums/`. Note: `CvAnalysisResult.Skills` is `List<UserSkillDto>` (user-scoped, has `UserId`); `SkillDto` (no user context) is kept for internal AI mapping only. No project dependencies.
- **`JobSearch.Persistence.Abstractions`** — Repository interfaces: `IJobRepository`, `IUserRepository`, `IUserProfileRepository`, `IUserSkillRepository`, `IUserJobMatchRepository`, `IJobSiteRepository`. Also holds its own persistence-layer DTOs in `DTOs/` (`UserPersistenceDto`, `UserProfilePersistenceDto`, `UserSkillPersistenceDto`, etc.) — these are separate from Application DTOs and never cross into Business. No dependencies.
- **`JobSearch.Business`** — Service implementations (`JobService`, `JobMatchService`, `UserProfileService`). `BusinessMapper` (internal static class in `Mapping/`) translates between Application DTOs and Persistence DTOs. References both abstractions layers; never references Persistence or AI directly.
- **`JobSearch.Persistence`** — EF Core 9 + SQLite. `AppDbContext` (with `DbSet<>` for all six entities) lives at `Repositories/AppDbContext.cs`. All six repository classes are fully implemented. `PersistenceMapper` (in `Mapping/`) maps between EF entities and Persistence DTOs. Only references `Persistence.Abstractions`.
- **`JobSearch.Scraping`** — Playwright (v1.60) browser automation + HtmlAgilityPack. Standalone — no project references.
- **`JobSearch.AI`** — Anthropic SDK (v5.10). Both `CvParser` (implements `ICvParser`) and `QuestionGenerator` (implements `IQuestionGenerator`) are fully implemented. `AddAiServices` reads the API key from configuration and throws `InvalidOperationException` if absent — set it via user-secrets before running.
- **`JobSearch.Email`** — MailKit/MimeKit (v4.17) email notifications. Standalone — no project references.
- **`JobSearch.Worker`** — .NET Generic Host (`BackgroundService`) orchestrating scrape → AI match → email. Can run as a Windows Service (`Microsoft.Extensions.Hosting.WindowsServices`).

### AI pipeline (`JobSearch.AI`)

Two AI services, both fully implemented:

**`CvParser`** (implements `ICvParser`):
1. PDF bytes → base64 → `DocumentContent` in an Anthropic `MessageParameters` (model: `claude-sonnet-4-20250514`, max 2000 tokens).
2. System/user prompts in `CvParserService/CvParserPrompts.cs` as `internal const string` constants. System prompt instructs Claude to return **only** a raw JSON object — no markdown or backticks.
3. Response JSON deserialized into internal models in `CvParserService/` (`CvAnalysisRaw`, `SkillRaw`, `WorkExperienceRaw`).
4. `CvParserService/CvAnalysisMapper.cs` maps raw models to Application DTOs, and builds `CvAnalysisResult.ClaudeReadyProfile` — a compact plaintext summary for feeding back into Claude for job-matching.
5. On parse failure, returns `CvAnalysisResult` with `IsSuccess = false` and `ErrorMessage` rather than throwing (except `OperationCanceledException`).

**`QuestionGenerator`** (implements `IQuestionGenerator`):
- Called by `UserProfileService.AnalyzeCvAsync` after `CvParser` succeeds.
- Sends `ClaudeReadyProfile` + skill/role summary to Claude (model: `claude-sonnet-4-6`, max 1000 tokens) to generate clarifying questions.
- Prompts in `QuestionGeneratorService/QuestionGeneratorPrompts.cs`. Response deserialized into `ClarifyingQuestionRaw` → `ClarifyingQuestionDto`.
- On failure, logs and returns empty list (never throws, except `OperationCanceledException`).

### WPF — DI and MVVM

DI is fully wired in `App.xaml.cs` via `Host.CreateDefaultBuilder()`. `App.OnStartup` builds the host, calls all `Add*Services()` extension methods, and resolves `MainWindow` from DI.

**DataContext**: `MainWindow` receives `UserProfileViewModel` via constructor injection and sets `DataContext = viewModel`. `UserProfileView` inherits this DataContext from its parent. Do not add `<UserControl.DataContext>` back to `UserProfileView.xaml` — XAML instantiation cannot inject constructor dependencies.

**Design-time data**: `UserProfileViewModel` has a `LoadDesignTimeData()` method with hardcoded `Skills` and `ClarifyingQuestions`. The call is currently commented out in the constructor. Do not uncomment it — real data comes from `AnalyzeCvAsync`.

#### MVVM conventions

- ViewModels and observable models derive from `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` and are declared `partial`.
- Backing fields use `[ObservableProperty]`; commands use `[RelayCommand]` with `CanExecute = nameof(...)` for guarded commands.
- Use `[NotifyPropertyChangedFor(nameof(Other))]` for derived properties and `partial void On<Prop>Changed(...)` hooks (e.g. to call `XCommand.NotifyCanExecuteChanged()`). Do not hand-roll `SetProperty`/`OnPropertyChanged` calls.
- `JobSearch.WPF/Models/` holds presentation-only types: `SkillItem`, `ClarifyingQuestionItem`. `ProficiencyLevel` and `AnswerType` live in `Application.Abstractions/Enums/` — do not re-add them to WPF.
- Dialog interactions go through `IDialogService` / `DialogService` in `JobSearch.WPF/Dialogs/` — do not call `MessageBox` directly from ViewModels.
- XAML styling: flat/minimal, light borders, no gradients or shadows, `CornerRadius="6"` to `"8"` on panels and controls. Primary accent color `#1565C0`.

### Configuration

- `appsettings.json` in each entry-point project holds stubs for: `ConnectionStrings`, `AnthropicSettings`, `WorkerSettings`, `EmailSettings`.
- Both WPF and Worker support `.AddUserSecrets<T>()` for local development secrets. Use user-secrets for the Anthropic API key and SMTP credentials.
- `WorkerSettings` is intended for the scrape→match→email pipeline scheduling interval.
