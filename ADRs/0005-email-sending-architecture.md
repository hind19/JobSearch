# ADR-0005: Email Sending Architecture

**Status:** Accepted
**Date:** 2026-07-23

## Context

`IEmailSender`, `EmailSettings`, and `EmailTemplateBuilder` in
`JobSearch.Email` were stubs with no implementation. `JobSearch.Worker`
already has a placeholder in `WorkerRun` (logs a warning that matches are
unnotified) waiting on this. Three requirements were set for this pass:

1. Every send attempt is logged in the database: recipient address,
   timestamp, body, and status (Sent/Failed).
2. Retries: 3 attempts, via Polly.
3. SMTP settings currently only live in `appsettings.json`, with no way
   to edit them without hand-editing config files — `JobSearch.WPF` needs
   a settings screen for this.

## Decisions

### 1. Non-secret SMTP settings move to the database (Option A)

`SmtpHost`, `SmtpPort`, `UseSsl`, `FromAddress`, `FromDisplayName`, and
`SmtpUsername` are stored in a new single-row `EmailSettings` table (see
updated `CreateDatabase.sql`), editable via a new WPF form
(`EmailSettingsView`/`EmailSettingsViewModel`, following the same
load-on-navigate/save pattern as `JobSitesView`). Single-row, not
per-user, per ADR-0003 (single-user local deployment) — the same
reasoning already applied to date. `appsettings.json`'s existing
`EmailSettings` section is retained only as **seed data**: if the DB
table is empty on startup, it's used once to populate the initial row,
then the DB becomes the live source of truth. Both `JobSearch.WPF` and
`JobSearch.Worker` read from the DB — the same shared-database pattern
already established for `Users`/`JobSites` (ADR-0002's "%ProgramData%"
path applies equally here, no new deployment concern).

### 2. SMTP password stays in user-secrets only

`SmtpPassword` (or an auth token) is **never** stored in the database and
**never** read or written by the WPF form. It's read directly from
configuration (`EmailSettings:SmtpPassword` via user-secrets/env var,
same convention as `AnthropicSettings:ApiKey`) at send time. The WPF form
shows a static note pointing to `dotnet user-secrets set` instead of a
password field — consistent with the project's existing rule ("Секреты —
только через dotnet user-secrets или переменные среды") and avoids
putting a credential in a database file that already has a broader access
surface than a single config store (both hosts, and anyone with
filesystem access to `%ProgramData%`).

### 3. Every send attempt is logged before and after, not just on success

A new `SentEmails` table (see `CreateDatabase.sql`) stores `UserId`,
`ToAddress`, `Subject`, `Body`, `Status` (Pending/Sent/Failed),
`AttemptCount`, `ErrorMessage`, `SentAt`, `CreatedAt`. The row is inserted
as `Pending` **before** the send attempt, then updated to `Sent` or
`Failed` after — not inserted only on success — so a process crash
mid-send still leaves an audit trail instead of silently vanishing.

### 4. Retry via Polly, 3 attempts, exponential backoff, no retry on auth failures

```csharp
services.AddResiliencePipeline("email-send", builder =>
{
    builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromSeconds(2),
        ShouldHandle = new PredicateBuilder()
            .Handle<SmtpCommandException>()
            .Handle<SmtpProtocolException>()
            .Handle<IOException>()
            .Handle<TimeoutException>()
    });
});
```

Polly was not weighed against alternatives — it's the established .NET
standard for exactly this case, integrates with DI directly
(`Microsoft.Extensions.Resilience`), and nothing about this project's
scale or constraints argues for a custom retry loop instead. Auth
failures (wrong credentials) are deliberately excluded from the retry
predicate — that's a configuration problem, not a transient one; retrying
it three times only delays an outcome that won't change.

### 5. A failed send does not abort the Worker run, and does not mark matches as notified

`IEmailSender.SendJobDigestAsync` returns a result indicating whether the
send succeeded. `WorkerRun` only calls `IJobMatchService.MarkAsNotifiedAsync`
if it did. If all 3 retries are exhausted, the matches remain
`WasNotified = false` — the *next* Worker run's existing unnotified-matches
query (`GetUnnotifiedAsync`) picks them up automatically. No separate
retry-queue mechanism is introduced; the existing notification-pending
state already serves that purpose across runs, on top of Polly's
within-run retries.

### 6. `send_digest_email` is not registered as an agent-loop tool

This was left open during the ADR-0004 tool design. Resolving it now:
sending an email is an irreversible, user-facing side effect that can't
be un-sent, unlike scraping or scoring a job. `WorkerRun` calls
`IEmailSender.SendJobDigestAsync` deterministically, in C#, after the
agent loop finishes — the model never decides whether or when to send it.
This is a narrower application of the same reasoning ADR-0004 already
uses for `save_job`/`score_relevance` guardrails: keep the
consequential/hard-to-reverse decision outside the model's discretion.

### 7. Email body is plain text, not HTML — rendered from an external template file

`Jobs.DescriptionRaw` (and other scraped/model-extracted fields feeding
the digest) is unsanitized text sourced from third-party pages. Rendering
it as HTML without a sanitization step is unnecessary injection surface
inside an email client, for no real benefit at this project's scale. This
matters more, not less, once a web version exists (see project status:
WPF → Blazor migration path) — a web-facing surface raises the cost of an
injection mistake compared to a single local desktop client, so the
plain-text choice is reinforced rather than revisited when that migration
happens.

Rather than building the plain-text body via string concatenation/
interpolation inside `EmailTemplateBuilder`, the body is rendered from an
external template file — `JobSearch.Email/Templates/JobDigestEmail.txt`,
copied to the output directory (`CopyToOutputDirectory`, same convention
as `Scripts/CreateDatabase.sql`) — using simple placeholder tokens
(`{{CandidateName}}`, `{{JobCount}}`, `{{JobListing}}`, etc.), not HTML
tags. `EmailTemplateBuilder` loads the file and replaces tokens; a
separate per-job sub-template (or a simple loop with a smaller inline
block for the repeating job entries) builds the `{{JobListing}}` section
from `List<UserJobMatchDto>`. This keeps the wording editable without a
recompile and mirrors the project's existing separation of content from
code (`LocalizationManager` + `ResourceDictionary` for UI strings) — no
templating engine dependency is needed for this; plain
`string.Replace`/token substitution is sufficient at this scale, and
adding one (e.g. Scriban) would be unjustified complexity for a single
digest email format.

`SentEmails.Body` stores the final rendered plain-text content — what was
actually sent, not the template or a reference to it.

## Consequences

- New tables: `SentEmails`, `EmailSettings` (see `CreateDatabase.sql`).
- New WPF screen (`EmailSettingsView`) and its ViewModel/service chain
  (`IEmailSettingsService` in Business, mirroring `IJobSiteService`).
- `JobSearch.Email` gains a real `EmailSender`, `EmailTemplateBuilder`,
  and a DB-backed settings read path instead of `IOptions<EmailSettings>`
  at send time (that binding is retained only for first-run DB seeding).
- New asset: `JobSearch.Email/Templates/JobDigestEmail.txt`, a plain-text
  template with token placeholders, copied to the output directory.
- New package dependency: `Microsoft.Extensions.Resilience` (Polly) in
  `JobSearch.Email`.
- `WorkerRun`'s existing `GetUnnotifiedAsync` + warning-log placeholder is
  replaced with an actual send + conditional `MarkAsNotifiedAsync`.
- `IEmailSender`'s method signature needs a return type carrying
  send-success (e.g. `EmailSendResult`), not just `Task` — a plain `Task`
  can't tell `WorkerRun` whether it's safe to mark matches notified.
