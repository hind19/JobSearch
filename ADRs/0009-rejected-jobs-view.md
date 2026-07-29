# ADR-0009: Rejected Jobs View

**Status:** Accepted
**Date:** 2026-07-27

## Context

The user has no visibility into jobs Claude analyzed and rejected. Every
scraped job goes through `ScoreRelevanceTool.score_relevance`, which
calls `JobMatchService.TryCreateMatchAsync(userId, jobId, score, reason,
ct)`. Today:

```csharp
// JobSearch.Business/Services/JobMatchService.cs
if (clampedScore < threshold)
    return null; // not an error — "below threshold" is a normal outcome
```

Claude computes a score and a reason for *every* job, but when the score
is below `AnthropicSettings.RelevanceThreshold`, both are discarded. The
job itself is already in `Jobs` (saved by `SaveJobTool` before scoring
happens), but nothing records *why* it didn't become a match, or *when*
it was analyzed. The user has asked to close this gap: they want to see,
per day, which jobs were rejected, why, with a clickable link back to the
posting.

This sits next to the existing Statistics screen (ADR-0006), which
reports aggregate match counts per job site. Rejected jobs are a
complementary, per-item view for a specific user — not another
aggregate — so it's added as a second tab on the same screen rather than
folded into the existing `DataGrid`.

## Decision

### Data model — new table, not a flag on `UserJobMatches`

New table `UserJobRejections`, structurally parallel to
`UserJobMatches` (same `UserId`/`JobId` pair, same cascade-delete
posture), but without `WasNotified`/`IsApplied` — those concepts don't
apply to a rejected job:

```sql
CREATE TABLE IF NOT EXISTS UserJobRejections (
    Id              TEXT    NOT NULL PRIMARY KEY,
    UserId          TEXT    NOT NULL,
    JobId           TEXT    NOT NULL,
    RelevanceScore  REAL    NOT NULL,
    RelevanceReason TEXT,
    AnalyzedAt      TEXT    NOT NULL,
    CONSTRAINT FK_UserJobRejections_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserJobRejections_Jobs
        FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserJobRejections_UserId_JobId
        UNIQUE (UserId, JobId)
);

CREATE INDEX IF NOT EXISTS IX_UserJobRejections_UserId_AnalyzedAt
    ON UserJobRejections (UserId, AnalyzedAt);
```

A separate table (rather than an `IsMatch` flag plus nullable columns on
`UserJobMatches`) keeps the two concepts — "recommended, pending
notification" vs. "analyzed and rejected" — from sharing one table's
indexes and query shape. This mirrors the ADR-0006 rationale: a
cross-cutting concern gets its own home rather than being bolted onto an
existing aggregate root.

`UNIQUE (UserId, JobId)` is safe: `CheckJobExistsTool` prevents the agent
from re-scraping/re-scoring a URL already present in `Jobs`, so
`score_relevance` runs at most once per job.

**Schema application:** manual. Per the user's explicit decision, this
ADR updates `CreateDatabase.sql` as the source of truth for new
installs, but the user will apply the equivalent DDL to their existing
local database by hand — same approach as ADR-0007/0008. No EF Core
migration mechanism is introduced by this ADR.

### `JobMatchService.TryCreateMatchAsync` now persists the rejection path

The method's signature and its two callers (`ScoreRelevanceTool`) are
unchanged. Internally, the below-threshold branch now writes a
`UserJobRejections` row instead of silently discarding the score/reason,
then still returns `null` (semantics for the caller are unchanged — "no
match was created" is still expressed the same way).

The existing job-existence guard (`_jobRepository.GetByIdAsync`, which
throws if the `jobId` wasn't produced by a prior `save_job` call in the
run) now runs for **both** branches, not just the match-creation one —
consistent validation regardless of outcome. This is a behavior change
from the current code (today the guard is skipped entirely when
below-threshold) and updates the existing unit tests accordingly.

### Reporting: new dedicated service, not an extension of `IJobMatchService`

`IJobRejectionService` (Application.Abstractions) / `JobRejectionService`
(Business), following the ADR-0006 precedent of a dedicated
read-oriented service per screen concern rather than growing
`IJobMatchService` with unrelated read methods:

```csharp
public interface IJobRejectionService
{
    Task<DateTime?> GetMostRecentAnalysisDateAsync(
        Guid userId, CancellationToken ct = default);

    Task<RejectedJobsPageDto> GetRejectedJobsAsync(
        Guid userId, DateTime date, int page, int pageSize,
        CancellationToken ct = default);
}
```

`GetMostRecentAnalysisDateAsync` backs the "auto-load last scan on open"
behavior (see UI section). `GetRejectedJobsAsync` is a single-day query,
not a range — filtering is "pick a day," not "pick an interval" (see
Open Questions resolution below).

### UI: `StatisticsView` becomes a `TabControl`

- **Tab 1** — existing per-site `DataGrid`, unchanged.
- **Tab 2 ("Отклонённые")** — new `DataGrid`: Ссылка / Причина
  отклонения / Дата анализа, with pagination (20 rows/page — see below)
  and a single `DatePicker` filter above the grid.

**Filter behavior (resolves open question #1 from the development
plan):** on screen open, the grid auto-loads the most recent day that
has any analyzed rejection (`GetMostRecentAnalysisDateAsync`), and the
`DatePicker` reflects that date. Changing the date reloads immediately —
no separate "Apply" button, no date-range picker. This is a single-day
filter, not an interval, superseding the original plan's date-range
proposal.

**Link behavior (resolves open question #2):** the "Ссылка" column is a
clickable `Hyperlink` bound to a `RelayCommand` that opens the URL in
the OS default browser via `Process.Start(new ProcessStartInfo(url) {
UseShellExecute = true })`.

**"Дата анализа" (resolves open question #3):** the moment
`score_relevance` is recorded for a below-threshold job —
`DateTime.UtcNow` captured inside `JobMatchService`, the same pattern
`FoundInRunAt` uses for matches. Not `Job.FoundAt` (scrape time).

**Pagination (resolves open question #4):** 20 rows per page, with
Next/Previous controls. Changing the filter date resets to page 1.
Pagination is enforced at the repository level (`Skip`/`Take` + a
separate `COUNT` for total pages) — not loaded-then-sliced in memory —
since a busy scraping day could otherwise pull an unbounded row set into
the WPF process.

**User identity plumbing:** `StatisticsViewModel.LoadAsync` gains a
required `Guid userId` parameter (previously none — the per-site tab
doesn't need one). `MainViewModel` now retains the `userId` passed to
`InitializeAsync` and forwards it when navigating to the Statistics
screen.

## Consequences

- New table `UserJobRejections`; no changes to `UserJobMatches` or its
  indexes.
- `JobMatchService.TryCreateMatchAsync` behavior change: the
  below-threshold path now always performs a job-existence check and a
  write, where previously it did neither. Existing unit tests for the
  below-threshold path are updated to assert the new calls instead of
  their absence.
- New interfaces/DTOs across all layers: `IUserJobRejectionRepository` +
  `UserJobRejectionPersistenceDto` + `RejectedJobsPagePersistenceDto`
  (Persistence.Abstractions), `IJobRejectionService` +
  `RejectedJobDto` + `RejectedJobsPageDto` (Application.Abstractions).
- `JobSearch.Worker` and the agent loop (ADR-0004) are otherwise
  untouched — `ScoreRelevanceTool`'s call site and contract don't change.
- `JobSearch.WPF`: `StatisticsView`/`StatisticsViewModel` gain the second
  tab, filter, pagination, and a browser-launch side effect; a new
  `RejectedJobRowItem` presentation model is added.
- Statistics screen (ADR-0006, per-site aggregates) is unaffected —
  Tab 1 is a straight carry-over of the existing view.
