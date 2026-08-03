# ADR-0010: PII Exclusion from Claude-Facing Flows

**Status:** Accepted
**Date:** 2026-08-02

## Context

`CvParserPrompts.cs`'s system prompt already states a rule for the
`claudeReadyProfile` field Claude itself generates during CV parsing:

> `claudeReadyProfile` rules: MUST NOT contain: full name, email, phone
> number, home address.

However, `CvAnalysisMapper.ToResult` does not use the `claudeReadyProfile`
value Claude returns. It discards it and rebuilds the field itself via
`BuildClaudeReadyProfile(raw)`, which interpolated `raw.FullName` directly
into the returned text:

```csharp
return $"""
    Candidate: {raw.FullName ?? "Unknown"}
    Location: {raw.Location ?? "not specified"}
    ...
    """;
```

The rule existed in the prompt but was never enforced in the code that
actually produces the persisted value. `CvAnalysisResult.ClaudeReadyProfile`
is not a leaf value — it is read back and sent to Claude again at multiple
points downstream:

- `IQuestionGenerator.GetClarifyingQuestionsAsync` (WPF CV-analysis flow)
- `IProfileEnricher.EnrichAsync` (profile save flow)
- The Worker agent loop's initial context (per
  `JobSearch.AI/docs/worker-agent-tool-design.md` — "The resolved user's
  `ClaudeReadyProfile` text ... loaded ... before starting the loop")

So the candidate's full name was being sent to Claude on every CV
follow-up call and on every vacancy-matching run, contradicting the
prompt's own stated rule, and contradicting the project's general
principle that personal data (full name, email, phone) must not be
processed or transmitted without explicit instruction.

This was found during a review specifically requested to exclude
candidate PII from the vacancy-analysis flow and from the
`ClaudeReadyProfile` field.

## Decision

1. **`BuildClaudeReadyProfile` no longer includes `FullName`.** This is
   the single point that builds the value persisted as
   `UserProfile.ClaudeReadyProfile`, so fixing it here fixes every
   downstream consumer (`QuestionGenerator`, `ProfileEnricher`, the Worker
   agent loop context) without touching each call site individually.

2. **`Location` is kept**, by explicit decision, because it's needed for
   job-matching relevance (city-level location). This is narrower than
   what the CV-parsing prompt asks Claude to avoid ("home address") —
   `Location` here is a city/region used for matching, not a street
   address. Revisit if `Location` ever starts capturing more precise
   address data.

3. **`CandidateInfo` (`FullName`/`Email`/`Phone`) is unchanged** and
   continues to flow into `UserProfileService.SaveProfileAsync`, which
   writes it to the `Users` table (not `UserProfile.ClaudeReadyProfile`).
   This is a human-triggered, explicit save action in the WPF flow (the
   user clicks "Save profile") and the data is used for the user's own
   account (e.g. digest email delivery) — not sent to Claude as part of
   analysis. This remains outside the scope of this ADR.

4. **General rule going forward:** personal data (full name, email,
   phone) must not be included in any text sent to Claude, or in any
   field whose contract is "text sent to Claude" (like
   `ClaudeReadyProfile`), unless the user has explicitly instructed
   otherwise for that specific case. New fields or prompt-context
   assembly code that touch `ClaudeReadyProfile` (or any future
   equivalent) must be checked against this rule before merging, not
   just described as following it.

## Consequences

- `CvAnalysisMapper.BuildClaudeReadyProfile` drops the `Candidate: ...`
  line. One-line change, no interface/DTO/schema changes.
- No changes to `CandidateInfo`, `CvParserPrompts.cs`, `IProfileEnricher`,
  `IQuestionGenerator`, or the Worker agent-loop design — the fix is
  isolated to the single function that builds the persisted profile text.
- Recommended (not yet implemented): a unit test on
  `CvAnalysisMapper.ToResult` asserting that
  `result.ClaudeReadyProfile` does not contain `raw.FullName`,
  `raw.Email`, or `raw.Phone` when those are non-null, so a future change
  to `BuildClaudeReadyProfile` that reintroduces PII fails a test instead
  of shipping silently.
- `CLAUDE.md`'s AI pipeline section gets a short note pointing at this
  ADR, so the rule is visible without having to already know this bug
  happened.

## Related

- ADR-0004 (Agent-Loop Architecture): defines that the Worker agent
  loop's initial context includes `ClaudeReadyProfile` — the flow this
  ADR's fix directly protects.
- `JobSearch.AI/docs/worker-agent-tool-design.md`: confirms
  `ClaudeReadyProfile` is loaded into the agent loop's initial user
  message before any tool calls happen.
