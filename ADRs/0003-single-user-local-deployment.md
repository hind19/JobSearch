# ADR-0003: Single-User Local Deployment; Multi-User Support Deferred to Future Blazor/Cloud Migration

**Status:** Accepted
**Date:** 2026-07-17

## Context

`JobSearch.WPF` runs as a local desktop application against a local SQLite
database. `JobSearch.Worker` shares that same local database. Both the UI
and the data store live on a single machine.

Concurrent use by multiple distinct users on separate machines against a
shared instance is not supported by this model, since the database itself
is local. The Worker identity-resolution strategy of picking the
most-recently-modified user (ADR-0002) is only valid if there is a single
meaningfully active user at a time.

## Decision

The current architecture explicitly targets single-user, local deployment:
one WPF instance, one local SQLite database, one Worker instance resolving
against that same database. Concurrent multi-user access is out of scope
for the current codebase — no per-user locking, session isolation, or
concurrent-edit conflict resolution is implemented or planned for this
phase.

Multi-user support is deferred to a future migration:

- UI moves from WPF to **Blazor**, enabling multiple concurrent client
  sessions against a shared backend.
- The database moves from local **SQLite to Azure SQL**, consistent with
  the migration path already noted in `JobSearch.Persistence`
  (`SQLite → Azure SQL`).
- Real authentication and per-user identity resolution replace the
  "last-modified user" heuristic from ADR-0002 at that point.

## Consequences

- Simplifies current implementation: no session/user isolation, locking, or
  auth infrastructure is required now.
- The Worker identity-resolution strategy in ADR-0002 is valid precisely
  because this deployment model guarantees single-user usage; it must be
  revisited when this ADR is superseded.
- Establishes an intentional, known migration path (WPF → Blazor,
  SQLite → Azure SQL) rather than leaving multi-user support as an
  open-ended TODO, so future work has a defined target instead of ad hoc
  patches bolted onto the desktop/local model.
- Interim feature work should avoid introducing assumptions that would need
  to be undone during the Blazor/Azure migration (e.g. no new local-only,
  single-machine assumptions beyond what SQLite already implies).
