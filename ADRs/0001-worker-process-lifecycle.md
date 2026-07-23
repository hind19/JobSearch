# ADR-0001: Worker Process Lifecycle Model

**Status:** Accepted
**Date:** 2026-07-16

## Context

`JobSearch.Worker` executes a periodic cycle: login (bypass) → load profile
+ active sites (in parallel) → scraping → matching → sending the email
digest. A process lifecycle model needs to be chosen.

## Decision

Worker is implemented as a single-run process, launched by an external
scheduler (Windows Task Scheduler in production, cron/systemd timer as an
alternative). There is no internal timer/BackgroundService — `Main()`
executes one full cycle and exits with a status code.

## Rationale

- Data (profile, active sites, jobs) is re-read from the database on every
  run regardless of the chosen model — keeping state resident in memory
  provides no benefit.
- A single-run process is simpler, isolates failures at the run boundary,
  and requires no overlap protection or graceful shutdown mid-operation.
- Consistent with the original architecture note
  "JobSearch.Worker — net9.0, Windows Service / cron".

## Consequences

- The run schedule is configured outside the code (Task Scheduler / cron),
  requiring a separate deployment step.
- A long-running BackgroundService model is out of scope for v1; may be
  revisited if sub-minute polling intervals become necessary.

## Related

- ADR-0004 (Agent-Loop Architecture for Worker Job Discovery & Matching):
  the job-discovery/matching stage inside this single run is implemented
  as a Claude agent loop, not a fixed step sequence. This does not change
  the single-run process model described here — the loop is internal to
  one `Main()` execution and still exits with a status code.
