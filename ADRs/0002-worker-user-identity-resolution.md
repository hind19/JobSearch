# ADR-0002: Worker User Identity Resolution via Last-Modified User in Database

**Status:** Accepted
**Date:** 2026-07-17

## Context

`JobSearch.Worker` runs unattended (Windows Service / cron trigger). There is
no interactive session available to present a login screen.

`JobSearch.WPF` currently resolves the acting user through `LoginViewModel`,
which supports a config-driven bypass (`LoginSettings:BypassEmail`) that
skips password validation and resolves `userId` via
`IUserProfileService.FindUserByEmailAsync`. This mechanism assumes a human
is present to open the login screen and is not directly usable by a headless
process.

Two alternatives were considered and rejected for sharing identity between
WPF and Worker:

- **`dotnet user-secrets`** — this is a dev-only mechanism backed by a JSON
  file under the interactive Windows user's `%APPDATA%`. A Windows Service
  typically runs under a different account (`LocalSystem`,
  `NetworkService`, or a dedicated service account) with a different
  `%APPDATA%`, so Worker would not reliably see what WPF wrote. It also
  repurposes a tool intended for deployment secrets to store mutable
  application state.
- **A shared state file** (e.g. under `%ProgramData%`) — solves the
  cross-account visibility problem but still conflates configuration/secret
  storage with mutable state, and requires manual atomic-write handling to
  avoid partial reads.

Real multi-user authentication is not implemented yet (see ADR-0003 for the
deployment model this decision assumes).

## Decision

Worker resolves the target user by querying the database, via
`IUserRepository`, for the user with the most recent modification timestamp
on their profile (e.g. `User.UpdatedAt` / `UserProfile.LastModifiedAt`).
No login, credential check, or config-based email lookup is performed by
Worker.

`JobSearch.WPF` is responsible for keeping this timestamp current whenever a
profile is created or edited — this is expected to be a natural side effect
of the existing `SaveProfileAsync` persistence path and requires no new UI
work beyond confirming the column is updated on every save.

## Consequences

- Worker has no dependency on the Windows account context or file-system
  location of any credential store. It behaves identically whether started
  interactively or under a service account, as long as it points at the
  same database as WPF.
- `dotnet user-secrets` retains its original purpose (deployment secrets)
  and is not repurposed for mutable state.
- This strategy is only correct because exactly one user is meaningfully
  "active" at a time in the current deployment model (see ADR-0003). It is
  not a multi-user identity mechanism.
- When real authentication is introduced, this resolution step is replaced
  by an explicit identity passed into Worker (e.g. a per-user scheduled job,
  a service principal, or a token-based context). The rest of the pipeline
  (load profile → load sites → scrape → match → notify) is unaffected by
  that future change.
- Requires a reliable `UpdatedAt` / `LastModifiedAt` column on the
  user/profile entity, updated on every save path. If not already present,
  it must be added.
