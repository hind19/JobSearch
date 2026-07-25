# ADR-0007: Job Application Tracking — Persistence Foundation Only

**Status:** Accepted
**Date:** 2026-07-24

## Context

While designing the job source statistics feature (ADR-0006), a gap
surfaced: nothing in the system lets the user mark a matched job as
"applied to." The system currently distinguishes only "found"
(`FoundInRunAt`) and "notified" (`WasNotified`/`NotifiedAt`) — whether
the user actually acted on a recommendation is untracked. This would be
a meaningfully stronger signal for future statistics (match → applied
conversion per site) than match count or relevance score alone.

Separately, the user raised a scope concern: the project has been
accumulating features beyond its original core (CV parsing → job
matching → notification) — Worker's agent loop, the email service, and
now statistics were all real needs, but each one expands the surface
area, and the user wants to deliberately pause that expansion rather than
keep pulling in "while we're at it" features indefinitely.

These two things point to the same resolution: capture the data model now
— so it isn't lost, and so it doesn't require a schema migration
mid-stream later, once Business/UI work resumes — without actually
building the Business logic or UI that would make the feature usable
today.

## Decision

**Persistence-layer foundation only.** `UserJobMatch` gains two fields:

- `IsApplied` (bool, default `false`)
- `AppliedAt` (nullable datetime)

This touches, and only touches:

- `CreateDatabase.sql` — two new columns on `UserJobMatches`.
- `UserJobMatch` entity (`JobMatch.cs`) — two new properties.
- `UserJobMatchConfiguration` — Fluent API config for the two properties.
- `UserJobMatchPersistenceDto` — two new properties, added as **optional
  constructor parameters with defaults** (`isApplied = false`,
  `appliedAt = null`), specifically so every existing call site
  (`PersistenceMapper`, `JobMatchService.TryCreateMatchAsync`) keeps
  compiling unmodified. No existing code needed to change for this ADR
  to land.
- `PersistenceMapper` — `ToDto`/`ToEntity` for `UserJobMatch` now carry
  the two new fields through.
- `IUserJobMatchRepository` — one new method, `MarkAppliedAsync(matchId,
  isApplied, ct)`, and its implementation. Nothing calls this method yet.

**Explicitly not done, and not to be started without a separate,
deliberate decision to resume this work:**

- No `Application.Abstractions` changes (no `IJobMatchService` method
  exposing this to Business/Worker/WPF).
- No `JobSearch.Business` changes beyond what's needed to keep existing
  code compiling (none — see above).
- No `JobSearch.WPF` UI — no checkbox, no button, no view changes. The
  job-browsing screen this would live on doesn't exist yet regardless
  (see original project status: "Просмотр вакансий в WPF — не
  реализовано").
- No changes to the statistics feature (ADR-0006) — "match → applied
  conversion rate" is a plausible future sixth metric, explicitly
  deferred alongside this.

## Consequences

- The database schema is ready for this feature whenever it's picked
  back up — no future migration needed just to add the columns.
- Nothing in the running system currently sets `IsApplied`/`AppliedAt` to
  anything but their defaults (`false`/`null`) — this ADR is inert by
  design until a follow-up decision explicitly resumes it.
- When resumed, the follow-up work is scoped narrowly: add
  `IJobMatchService.MarkAppliedAsync` (Application.Abstractions +
  Business, wrapping the repository method that already exists), then
  the UI affordance once the job-browsing screen exists to put it on.
- This ADR is also a marker of the broader decision it responds to: new
  feature scope is being deliberately paused at "data model only" going
  forward, rather than each new idea automatically expanding into full
  Business+UI work. Future asks in this vein should default to the same
  pattern (persistence foundation, explicit pause) unless there's a
  specific reason to build the full vertical slice immediately.
