# ADR-0008: Jobs.ExternalId — Reserved for Future Use, Nullable

**Status:** Accepted
**Date:** 2026-07-25

## Context

`Jobs.ExternalId` was part of the original schema (source-site job
identifier, distinct from our internal `Job.Id`, from `Url`, and from
`UrlHash`), but was never actually wired up: the `save_job` agent tool
(ADR-0004, `worker-agent-tool-design.md`) doesn't request it from Claude,
and `SaveJobTool.cs` passes `externalId: null` unconditionally. Nothing
in the system currently reads or writes a real value for this column.

This was discovered to be worse than dead weight: the column was defined
`NOT NULL` with a `UNIQUE (JobSiteId, ExternalId)` constraint, while the
only value ever written to it is `null`. Every call to
`JobIngestService.CreateAsync` — i.e. every job the Worker agent loop
saves — was at risk of failing a `NOT NULL` constraint violation at the
database level. This is a correctness bug independent of whether the
field is ever put to real use.

Three options were considered for the field's future:

1. Drop the column — it's unused, remove the dead weight.
2. Wire it up now — add it to the `save_job` tool schema, have Claude
   extract it, decide how it's used (secondary dedup key? display-only?).
3. Keep the column, fix the immediate bug (make it nullable), leave it
   unpopulated — same posture as ADR-0007's "persistence foundation,
   development paused."

## Decision

**Option 3.** Consistent with the project-wide decision in ADR-0007 to
deliberately pause feature-scope growth rather than pull in "while we're
here" work: `ExternalId` stays in the schema as a placeholder for a
plausible future use, but nothing beyond the schema fix is done now.

**Possible future use, for whoever picks this up:**
- **Secondary/more stable dedup key.** `UrlHash` is derived from the
  full URL, which can include tracking query parameters that some sites
  vary per page load for what is otherwise the same posting. If a site's
  own job ID (embedded in the URL path or page markup) is stable across
  those variations, `ExternalId` could catch duplicates `UrlHash` alone
  would miss.
- **Display/reference value.** Independent of dedup, showing "Posting ID:
  12345" in a future job-browsing UI can help a user cross-reference a
  saved listing against the source site directly, without relying on a
  URL that may 404 after the posting is taken down.

Neither use is implemented. If picked up later, it would require: adding
`externalId` to the `save_job` tool's input schema, deciding whether
`JobIngestService` should also check it for dedup (alongside or instead
of `UrlHash`), and — separately — whatever UI would display it.

## Immediate fix (this ADR, not deferred)

`Jobs.ExternalId` becomes nullable — `TEXT` without `NOT NULL` — in
`CreateDatabase.sql`. This is not part of the "future use" deferral; it's
a correctness fix for the constraint-violation risk described above,
landing now regardless of when (or whether) the field is ever populated.

The `UNIQUE (JobSiteId, ExternalId)` constraint is kept as-is. SQL NULL
semantics (SQLite included) treat every `NULL` as distinct from every
other `NULL` for uniqueness purposes — so any number of rows with
`ExternalId = NULL` for the same `JobSiteId` can coexist without
violating the constraint. The constraint only starts actually enforcing
uniqueness once real values are written, which is exactly the behavior
wanted if/when this field is picked back up.

## Consequences

- Fixes a real bug: job saves no longer risk failing on a `NOT NULL`
  violation for a column nothing populates.
- No application code changes — `SaveJobTool.cs` already passes `null`;
  the schema now permits what the code was already doing.
- The column remains present and documented, not deleted — a future
  implementer doesn't have to reconstruct the original schema intent
  from scratch, and no data migration is needed if/when it's populated.
- Matches the posture set by ADR-0007: this project is deliberately
  distinguishing "persistence-layer placeholder, explicitly paused" from
  "implemented feature," and marking each instance with its own ADR
  rather than letting scope quietly expand without a paper trail.
