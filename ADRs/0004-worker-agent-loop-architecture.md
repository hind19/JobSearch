# ADR-0004: Agent-Loop Architecture for Worker Job Discovery & Matching

**Status:** Accepted
**Date:** 2026-07-21

## Context

Two AI-integration models were compared for how Claude participates in the
application:

- **Point-solution calls** (already used throughout `JobSearch.WPF` via
  `ICvParser`, `IQuestionGenerator`, `IProfileEnricher`): a C# method calls
  Claude once with a fixed prompt, gets a structured result back, and
  ordinary deterministic code decides what happens next. Claude has no
  visibility into, or control over, the surrounding pipeline.
- **Agent loop** (tool-calling): Claude is given a goal and a set of tools
  (JSON-schema functions it may call) and itself decides, turn by turn,
  which tool to call, how many times, and when the goal is complete. This
  was the model originally sketched for the whole project (`parse_cv`,
  `scrape_jobs`, `check_db`, `rank_job`, `save_to_db`, `send_email`,
  `ask_user`, driven by "Claude составляет план" — see the original
  project discussion).

Relevant project context that shaped this decision:

- **Educational purpose.** This project exists to learn how to design AI
  systems integrated into a .NET application. Deliberately using different
  integration models in WPF vs. Worker, matched to where each is
  appropriate, is itself a learning outcome, not incidental complexity.
- **Small scale.** The realistic deployment is a single active user (see
  ADR-0003) and fewer than 10 job sites per run. This removes the main
  practical objection to an agent loop — the cost and latency of multiple
  sequential round-trips to the Anthropic API per run is negligible at
  this scale, and Worker is not a real-time process (single run per
  schedule tick, per ADR-0001).
- **Control boundary differs by host.** `JobSearch.WPF` is where a human
  reviews and explicitly confirms AI output before it's persisted (CV
  analysis results are only written on `SaveProfileAsync`, which the user
  triggers). `JobSearch.Worker` runs unattended — there is no human in the
  loop to catch a bad result before it's written to the database or
  emailed to the user.

That last point cuts both ways: it's exactly what makes an agent loop
*viable* for Worker (no user available to manually sequence steps, so
letting Claude plan is a genuine advantage rather than a UX regression),
and exactly what makes hallucination in that loop *dangerous* (a bad
tool-call result reaches the database and the user's inbox with no
review step). The decision below treats both sides as first-class.

## Decision

`JobSearch.Worker`'s job discovery, scraping, and matching stage is
implemented as a **Claude agent loop** (tool-calling), replacing the
step-by-step deterministic C# pipeline that had been sketched for this
part of `WorkerRun`. `JobSearch.WPF` is explicitly **not** changed — it
keeps using point-solution AI calls with a human-confirmed save step, per
the control-boundary reasoning above.

The agent loop's tools wrap the same underlying services already designed
under ISP (`IJobSiteQueryService`, `IJobLinksScraper`, `IJobIngestService`,
`IJobMatchService`, `IEmailSender`) — those contracts and their
implementations are unchanged; only how they get invoked changes, from
direct calls in `WorkerRun` to tool calls issued by Claude.

The following guardrails are adopted as mandatory design constraints for
the tool set and the loop itself, not as later hardening:

1. **Structured tool outputs.** Every tool that returns job/relevance data
   returns a JSON object matching a fixed schema (e.g. via Anthropic SDK
   structured tool results), never free text that downstream code has to
   parse loosely.
2. **No trusting model-supplied identifiers on write.** `save_to_db` does
   not accept a job's identity (URL, hash) as asserted by the model in
   conversation text. The C# tool implementation recomputes/validates
   identifiers (e.g. `UrlHash`) from data obtained by the corresponding
   `scrape_jobs` tool call earlier in the *same* run, closing the door on
   the model inventing or misremembering a job between turns.
3. **Hard iteration cap.** The loop enforces a fixed maximum number of
   tool calls per run, independent of whether Claude "thinks" it needs
   more — protects against both runaway loops and open-ended hallucinated
   exploration.
4. **Relevance threshold enforced in C#, not by model judgment.**
   `AnthropicSettings:RelevanceThreshold` (already in configuration) is
   compared numerically in code before a `UserJobMatch` is created. A
   tool call asserting "this is relevant" is not sufficient on its own.
5. **Full transcript logging.** Every tool call, its arguments, and its
   result are logged per run, so a suspected hallucination can be traced
   back to exactly what Claude saw and decided, rather than only seeing
   the final DB/email side effect.

## Consequences

- A new AI-layer component is introduced in `JobSearch.AI`: an agent-loop
  orchestrator using the Anthropic SDK's tool-use support, plus the tool
  implementations (`scrape_jobs`, `check_db`, `rank_job`, `save_to_db`,
  `send_email`), each a thin wrapper over an existing service interface.
- The previously sketched `IJobDetailsParser` and `IJobRelevanceScorer`
  point-solution interfaces (proposed but not yet implemented) are folded
  into tool implementations instead of being separately, directly called
  from `WorkerRun` — the earlier flowchart-style pipeline sketch for this
  stage is superseded by this ADR.
- `WorkerRun.ExecuteAsync`'s remaining TODOs (scrape → parse → match →
  notify) collapse into a single step: invoke the agent loop for the
  resolved user, once profile and active sites are loaded. The existing
  early-return guardrails (missing profile / no active sites, see prior
  session) still gate entry into that step.
- Testing this stage moves from step-level unit tests (mocking each
  service call in a fixed sequence) to transcript/scenario-based testing
  (fixed or replayed tool-call sequences), since the real call order is
  determined by the model at runtime, not by code.
- `JobSearch.WPF` requires no changes. The solution now intentionally
  contains two different AI-integration models side by side — point-
  solution in WPF, agent loop in Worker — chosen per the educational
  purpose of the project.
- If the realistic scale assumption in ADR-0003 changes (multi-user,
  higher job-site counts), the cost/latency tradeoff of the agent loop
  should be re-evaluated; this ADR's scale argument does not automatically
  hold at a larger scale.

## Related

- ADR-0001 (Worker Process Lifecycle Model): the agent loop runs entirely
  within the single process run described there — it does not introduce
  a second process or a background loop of its own.
- ADR-0003 (Single-User Local Deployment): the small-scale assumption
  (1 user, <10 sites) is the basis for treating the agent loop's
  cost/latency profile as acceptable.
