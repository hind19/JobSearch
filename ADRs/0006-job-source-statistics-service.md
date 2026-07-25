# ADR-0006: Job Source Statistics Service

**Status:** Proposed (structural question below to be finalized before
implementation, after the Email service is done — see ADR-0005)
**Date:** 2026-07-23

## Context

The user wants visibility into which configured job site each
*recommended* (matched) job actually came from — by site **name**, not
just a raw URL — so they can judge which sites are worth keeping active.

Two things are already true in the existing schema, without any change:

- `Jobs.JobSiteId` is a foreign key to `JobSites.Id`, and `JobSites.Name`
  already gives a human-readable site name. "Source by name" is a join
  away, not a new column.
- `FK_Jobs_JobSites` uses `ON DELETE RESTRICT` (see `CreateDatabase.sql`)
  — a `JobSite` with historical `Jobs` can't be deleted. This wasn't
  designed with statistics in mind, but it incidentally protects the
  integrity of any future source-based reporting: historical data can
  never become orphaned/anonymous by a site being removed later.

What doesn't exist yet is any aggregation/reporting over this data — e.g.
"how many matched jobs came from each site," to help the user evaluate
site configuration quality.

## Decision (proposed)

**Recommendation: a new Business-layer service, not a separate project.**

`IJobStatisticsService` (Application.Abstractions), backed by aggregate
query methods added to `IUserJobMatchRepository`/`IJobRepository`
(Persistence) — e.g. `GROUP BY JobSiteId` counts and averages — not a new
`JobSearch.Statistics` project.

Rationale: at this project's actual scale (ADR-0003 — one user, fewer
than 10 job sites), the feature is a small number of aggregate queries
over data that already exists. A separate project brings its own DI
wiring, its own place in the dependency graph, and a versioning/deployment
surface — overhead that isn't justified by "GROUP BY JobSiteId" as the
actual complexity involved. This mirrors the same scale-based reasoning
already used for the agent loop (ADR-0004) and the single-row
`EmailSettings` table (ADR-0005): match the architecture to the real
workload, not to how large the feature sounds by name.

Consumption is `JobSearch.WPF`-only — a new read-only "Statistics"
screen. `JobSearch.Worker` has no reason to consult this mid-run; it's a
reporting feature for the human, not an input to scraping/matching
logic.

No new "source" column or denormalization on `Jobs`/`UserJobMatches` is
needed — `JobSiteId` already provides the link. Copying `JobSites.Name`
onto `Jobs` directly would only introduce a stale-copy risk if a site's
name is ever edited, for no query-performance benefit at this scale.

## Open questions (why this is Proposed, not Accepted)

1. **Exact metrics.** Minimum ask is "match count per site." Worth
   considering alongside it: average relevance score per site (signal
   quality, not just volume), jobs-scraped-vs-jobs-matched ratio per site
   (how selective/noisy a site is), most recent match date per site
   (is a site still producing anything).
2. **Time-windowing.** All-time totals only, or a rolling window (e.g.
   last 30 days)? Matters once Worker has accumulated many runs — an
   all-time count doesn't tell the user whether a site's *recent*
   performance has changed.
3. **Confirm the separate-project question explicitly.** The
   recommendation above is a default, not a final call — the user raised
   "separate project" themselves, so this should be revisited on purpose
   before implementation starts, not treated as settled by omission.

## Consequences (if the recommendation above is confirmed as-is)

- New interface `IJobStatisticsService` in `Application.Abstractions`.
- New read-only aggregate methods on existing repository interfaces
  (`IUserJobMatchRepository`/`IJobRepository`) — no new tables.
- New `JobSearch.WPF` view/ViewModel — deferred, no work starts until
  this ADR's open questions are resolved.
- No changes to `JobSearch.Worker`, the agent loop (ADR-0004), or the
  email service (ADR-0005) — this is purely additive reporting on data
  those already produce.
