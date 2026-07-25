# ADR-0006: Job Source Statistics Service

**Status:** Accepted
**Date:** 2026-07-23 (proposed) / 2026-07-24 (accepted)

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

## Decision

**Confirmed: a new Business-layer service, not a separate project.**

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

## Metrics (confirmed)

Five metrics, computed per job site via a single aggregate query (`Jobs`
LEFT JOIN `UserJobMatches`, grouped by `JobSiteId`):

1. **Jobs scraped per site** — volume baseline.
2. **Matches created per site** — the original ask: which sites the
   recommended jobs actually come from.
3. **Match rate per site** (matches / scraped) — signal quality: a site
   can scrape a lot and match little (noisy), or scrape little and match
   most (well-targeted).
4. **Average relevance score per site** — a second signal-quality angle;
   high match *rate* with low average *score* still isn't a great site.
5. **Most recent match date per site** — whether a site has gone quiet,
   not just how it performed historically.

`LEFT JOIN` (not `INNER JOIN`) is required so a site with zero matches
still appears with `MatchesCount = 0`, rather than being silently
excluded from the report.

## Time-windowing: all-time totals for v1

No date-range parameter in the first version. Rolling-window filtering
(e.g. last 30 days) is a real future need once Worker has accumulated
enough run history for "recent" to diverge meaningfully from "all-time,"
but there isn't enough history yet for that distinction to matter — added
now, it would be speculative complexity with nothing real to validate it
against. Revisit as a fast-follow once multi-week run data exists.

## Consequences

- New interfaces: `IJobStatisticsRepository` (Persistence.Abstractions)
  and `IJobStatisticsService` (Application.Abstractions). The repository
  is dedicated rather than added to `IJobRepository`/
  `IUserJobMatchRepository`, because the aggregate query spans both
  `Jobs` and `UserJobMatches` — a cross-cutting concern belongs in its
  own repository, not bolted onto either aggregate root's.
- New DTOs: `JobSiteStatisticsPersistenceDto`, `JobSiteStatisticsDto`.
- New `JobStatisticsService` (Business) — thin wrapper, `BusinessMapper`
  gains a matching `ToDto` entry.
- New `JobSearch.WPF` read-only view/ViewModel (`StatisticsView`) — no
  create/edit/delete, so no popup/validation machinery needed, unlike
  `JobSitesView`. Fourth `Home` navigation card, same pattern as the
  Email settings card.
- No new tables — no changes to `CreateDatabase.sql`.
- No changes to `JobSearch.Worker`, the agent loop (ADR-0004), or the
  email service (ADR-0005) — purely additive reporting on data those
  already produce.
