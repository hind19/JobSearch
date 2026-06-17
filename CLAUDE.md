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

# User Secrets (Worker project only — for API keys, SMTP credentials)
dotnet user-secrets set "AnthropicSettings:ApiKey" "<value>" --project JobSearch.Worker
dotnet user-secrets list --project JobSearch.Worker
```

There are no test projects yet.

## Architecture

This is a .NET 9 job-search automation solution with two entry points: a WPF desktop UI (`JobSearch.WPF`) and a `JobSearch.Worker` background service that can be installed as a Windows Service. The codebase is currently in early scaffolding — all service interfaces, repository interfaces, Business service implementations, and the Worker pipeline are empty stubs. Domain entity models and `DbContext` have not yet been created.

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

- **`JobSearch.Application.Abstractions`** — Application-layer service interfaces (`IJobService`, `IUserProfileService`, `IJobMatchService`). No dependencies. This is the boundary the WPF layer should program against.
- **`JobSearch.Persistence.Abstractions`** — Repository interfaces (`IJobRepository`, `IUserRepository`, `IUserJobMatchRepository`, `IJobSiteRepository`). No dependencies.
- **`JobSearch.Business`** — Service implementations (`JobService`, `JobMatchService`, `UserProfileService`). References both abstractions layers; never references Persistence or AI directly.
- **`JobSearch.Persistence`** — EF Core 9 + SQLite implementation of repository interfaces. Domain entity models and `DbContext` belong here. Only references `Persistence.Abstractions`.
- **`JobSearch.Scraping`** — Browser automation with Playwright (v1.60) + HTML parsing with HtmlAgilityPack. Standalone — no project references.
- **`JobSearch.AI`** — AI job-matching via `Anthropic.SDK` (v5.10). References Application.Abstractions, Persistence.Abstractions, and Scraping.
- **`JobSearch.Email`** — Email notifications with MailKit/MimeKit (v4.17). Standalone — no project references.
- **`JobSearch.Worker`** — .NET Generic Host (`BackgroundService`) that orchestrates the automated pipeline (scrape → AI match → email). Can be deployed as a Windows Service (`Microsoft.Extensions.Hosting.WindowsServices`). UserSecretsId: `dotnet-JobSearch.Worker-7f89b472-eadf-4203-a824-f76c74ab65c6`.

### WPF MVVM conventions

- `JobSearch.WPF` targets `net9.0-windows` with `<UseWPF>true</UseWPF>`.
- XAML styling: flat/minimal, light borders, no gradients or shadows, `CornerRadius="6"` to `"8"` on panels and controls. Primary accent color is `#1565C0`.
- The MVVM layer uses the `CommunityToolkit.Mvvm` NuGet package (referenced by `JobSearch.WPF.csproj`). ViewModels and observable models derive from `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` and are declared `partial`; backing fields use `[ObservableProperty]` and commands use `[RelayCommand]` (with `CanExecute = nameof(...)` for guarded commands) so the source generator produces the public properties/commands. Use `[NotifyPropertyChangedFor(nameof(Other))]` for derived properties and `partial void On<Prop>Changed(...)` hooks (e.g. to call `XCommand.NotifyCanExecuteChanged()`) instead of hand-written `SetProperty`/`OnPropertyChanged` calls. There is no `JobSearch.WPF/Infrastructure/` folder anymore — do not recreate a hand-rolled `ObservableObject`/`RelayCommand`.
- ViewModels and models in the WPF project are presentation-layer types only — `JobSearch.WPF/Models/` holds `SkillItem`, `ClarifyingQuestionItem`, `ProficiencyLevel`, and `AnswerType`. Do not add references to `JobSearch.AI` or place domain logic here; persistence entity models belong in `JobSearch.Persistence`.
- The `UserProfileViewModel` currently wires its `DataContext` directly in XAML (`<vm:UserProfileViewModel/>`) and calls `LoadDesignTimeData()` in its constructor. When DI is introduced, the DataContext binding must move to `App.xaml.cs` (or a ViewModelLocator) — ViewModel constructors cannot accept service dependencies while XAML instantiation is in use.
- DI has not yet been wired in `App.xaml.cs` — it is currently an empty `Application` subclass.

### Configuration

- `appsettings.json` in each entry-point project (`Worker`, `WPF`) holds section stubs: `ConnectionStrings`, `AnthropicSettings`, `WorkerSettings`, `EmailSettings`.
- The Worker project uses .NET User Secrets for local development of sensitive values (API keys, SMTP credentials). The WPF project does not have User Secrets configured — add secrets to the Worker's secret store and expose them via shared configuration if needed.
- `WorkerSettings` is intended for scheduling interval configuration (e.g., how often the scrape→match→email pipeline runs).
