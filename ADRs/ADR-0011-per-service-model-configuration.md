# ADR-0011: Per-Service Model Configuration for Claude API Calls

**Status:** Accepted
**Date:** 2026-08-02

## Context

`JobSearch.AI` makes five distinct kinds of Claude API calls, each in its
own class: `CvParser`, `QuestionGenerator`, `ProfileEnricher`,
`SelectorDetector`, `JobSearchAgent`. All five hardcoded the same model
as a private constant:

```csharp
private const string Model = "claude-sonnet-4-6";
```

`AnthropicSettings.Model` already existed as a bindable config value
(`services.Configure<AnthropicSettings>(...)` in `AiServiceExtensions.
AddAiServices`, populated in both `appsettings.json` files), but nothing
read it — the config was dead, and every call site was locked to one
model regardless of task.

Reviewing the five call sites against their actual usage patterns showed
they don't belong on the same model:

- **`QuestionGenerator`, `ProfileEnricher`** (`JobSearch.WPF` only) —
  low-complexity text generation/rewriting over already-structured
  input, short output, always reviewed by the user before it's saved.
- **`CvParser`, `SelectorDetector`** (`JobSearch.WPF` only) — extraction
  over unstructured input (PDF resume; arbitrary third-party HTML) where
  accuracy materially affects downstream data quality, but still
  human-reviewed before persistence.
- **`JobSearchAgent`** (`JobSearch.Worker` only) — multi-turn tool-calling
  agent loop that scrapes, parses, scores, and writes to the database and
  triggers an email digest **with no human in the loop** (see ADR-0004:
  "no user available to manually sequence steps ... exactly what makes
  hallucination in that loop dangerous"). A bad result here reaches the
  database and the user's inbox directly.

At this project's deployment scale (ADR-0003: one user, fewer than 10
job sites, one Worker run per day), the absolute dollar cost of any of
these calls is negligible regardless of model tier — the real
optimization isn't minimizing spend, it's not paying a quality premium
where a cheaper model already clears the bar, and not economizing on the
one call where there's no human safety net.

Separately, `WorkerSettings` and `EmailSettings` were audited for the
same "does this host actually use this key" question, since
`WorkerSettings:MaxAgentToolCalls` already existed as a Worker-only key
absent from `JobSearch.WPF/appsettings.json` — an existing precedent for
per-host config trimming that this ADR generalizes.

## Decision

### 1. `AnthropicSettings.Model` → `AnthropicSettings.Models` (per service)

```csharp
public class AnthropicSettings
{
    public int MaxTokens { get; set; }
    public int RelevanceThreshold { get; set; }
    public AnthropicModelSettings Models { get; set; } = new();
}

public class AnthropicModelSettings
{
    public string CvParser { get; set; } = string.Empty;
    public string QuestionGenerator { get; set; } = string.Empty;
    public string ProfileEnricher { get; set; } = string.Empty;
    public string SelectorDetector { get; set; } = string.Empty;
    public string JobSearchAgent { get; set; } = string.Empty;
}
```

Each of the five `JobSearch.AI` classes now takes `IOptions<AnthropicSettings>`
in its constructor and reads its own `Models.*` value instead of a
hardcoded constant. `MaxTokens` per call site is left as-is (each service
has a materially different value — 8192/1000/1024/1024/4096 — and
collapsing them into one config number would misrepresent the actual
per-task budget); only the model selection moves to config.

### 2. Model assignment per service

| Service | Host | Task profile | Model |
|---|---|---|---|
| `QuestionGenerator` | WPF | Low complexity, short output, human-reviewed | Haiku 4.5 |
| `ProfileEnricher` | WPF | Low complexity, rewriting, human-reviewed | Haiku 4.5 |
| `CvParser` | WPF | Medium complexity, PDF vision + structured extraction, human-reviewed | Sonnet 5 |
| `SelectorDetector` | WPF | Medium-high complexity, arbitrary/large HTML DOM, error cost compounds across every future scrape of that site | Sonnet 5 |
| `JobSearchAgent` | Worker | High complexity, multi-step tool-calling, **no human review**, writes directly to DB/email | Opus 4.8 |

Rationale is cost/effectiveness, not cost minimization: where the task is
simple and reviewed, the cheaper model is strictly sufficient and the
premium buys nothing. Where the task is complex or unattended, the
absolute per-run dollar difference between tiers is negligible at this
project's scale (ADR-0003), so the choice is made entirely on quality —
this is most pronounced for `JobSearchAgent`, the one call site where a
hallucination has no human backstop before it reaches persistent state.

### 3. `appsettings.json` split by actual host usage

`JobSearch.WPF/appsettings.json` keeps `Models.CvParser`,
`Models.QuestionGenerator`, `Models.ProfileEnricher`,
`Models.SelectorDetector` — `JobSearchAgent` is omitted (WPF never
resolves `IJobSearchAgent`).

`JobSearch.Worker/appsettings.json` keeps only `Models.JobSearchAgent` —
the other four keys are omitted (`WorkerRun` never calls
`ICvParser`/`IQuestionGenerator`/`IProfileEnricher`/`ISelectorDetector`
directly; the fact that `UserProfileService` — which these are
constructor dependencies of — still gets instantiated in Worker's
container because `WorkerRun` calls `IUserProfileService.GetUserAsync`/
`GetProfileAsync` is not a problem: construction just leaves those
`_model` fields as empty strings, which are never sent to the API since
the methods that would use them are never called from Worker).

This generalizes the existing `WorkerSettings:MaxAgentToolCalls`
precedent (already Worker-only) to the rest of `WorkerSettings`:
`ScheduleTime`, `DelayBetweenRequestsMs`, and `MaxPagesPerSite` are
currently read by no code in either host (schedule lives in the OS task
scheduler per ADR-0001; scraping throttle/page-cap are unimplemented),
and are conceptually Worker-only regardless (WPF does not scrape) — the
entire `WorkerSettings` block is removed from
`JobSearch.WPF/appsettings.json`.

`EmailSettings` is *not* split — it remains identical in both files,
deliberately. Unlike `AnthropicSettings`/`WorkerSettings`, it's consumed
as first-run DB-seed data (`EmailSettingsSeedOptions`, per ADR-0005 §1)
by whichever host happens to call `EmailSettingsService.GetAsync()`
first with no existing row — either WPF (user opens the settings screen)
or Worker (first unattended send attempt). Both copies must agree so
seeding doesn't depend on run order.

## Consequences

- `AnthropicSettings.Model` (single string) removed; `AnthropicSettings.
  Models` (per-service) added. No change to `RelevanceThreshold` or
  `MaxTokens` — out of scope for this ADR.
- `CvParser`, `QuestionGenerator`, `ProfileEnricher`, `SelectorDetector`,
  `JobSearchAgent` each gain an `IOptions<AnthropicSettings>` constructor
  dependency; each replaces its `private const string Model` with an
  instance field read from `Models.*`.
- No DI registration changes — `services.Configure<AnthropicSettings>(...)`
  in `AiServiceExtensions.AddAiServices` already binds the full section,
  including the new nested `Models` object.
- `JobSearch.WPF/appsettings.json` and `JobSearch.Worker/appsettings.json`
  diverge further: each carries only the `AnthropicSettings:Models` keys
  and (`WorkerSettings`) block its own host's code path actually reads.
  `EmailSettings` stays identical in both, and `ConnectionStrings`/
  `Logging` are unaffected by this ADR.
- Recommended, not yet implemented: a fail-fast check at startup (same
  place `AddAiServices` currently throws on a missing `ApiKey`) verifying
  every `Models.*` key the *current* host actually needs is non-empty —
  today a missing/misspelled key only surfaces as an API error (empty
  `Model` string) on first real call, not at process start.

## Related

- ADR-0004 (Agent-Loop Architecture): the "no human in the loop" framing
  that drives the `JobSearchAgent` → Opus decision here.
- ADR-0003 (Single-User Local Deployment): the scale assumption that
  makes absolute per-tier cost differences negligible, shifting the
  cost/effectiveness tradeoff toward quality rather than price.
- The prior ADR documenting the `ClaudeReadyProfile` PII fix (filed in
  this session as "ADR-0008" before the numbering collision described
  above was discovered) — renumber that file to whatever slot is
  actually free in the 0008–0010 range before adding either to the repo.
