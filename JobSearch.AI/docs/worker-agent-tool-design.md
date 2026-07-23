# Worker agent tool set — design (no implementation)

Companion to ADR-0004. Defines the tool schemas the Worker's Claude agent
loop will use, and exactly what existing C# each tool wraps. No code yet —
this is the contract to implement against.

## What's given to Claude as context, not as a tool call

To keep the tool count low and avoid wasting turns, the agent loop's
initial user message already contains (loaded by `WorkerRun` *before*
starting the loop, using services we've already built):

- The resolved user's `ClaudeReadyProfile` text (`IUserProfileService.GetProfileAsync`)
- The list of active job sites — `Id`, `Name`, `BaseUrl` only
  (`IJobSiteQueryService.GetAllActiveAsync`) — capped at <10 per ADR-0004's
  scale assumption, so this fits comfortably in context
- The configured relevance threshold (`AnthropicSettings:RelevanceThreshold`)

Claude never has to "ask" for these — they're just there, which is also
why there's no `get_active_job_sites` or `get_user_profile` tool below.

## Tool 1 — `scrape_job_links`

Wraps `IJobLinksScraper.ScrapeLinksAsync`. Plain C#, no AI inside the tool.

```json
{
  "name": "scrape_job_links",
  "description": "Get the list of job posting URLs currently listed on a given active job site.",
  "input_schema": {
    "type": "object",
    "properties": {
      "jobSiteId": { "type": "string", "format": "uuid" }
    },
    "required": ["jobSiteId"]
  }
}
```

**Returns:** `{ "links": string[] }`

**Guardrail hook:** every URL returned here is added to an in-memory,
run-scoped allow-list maintained by the C# orchestrator (not by Claude).
`fetch_job_page` and `save_job` (below) check against this list — a URL
Claude didn't get from this tool in *this* run can't be fetched or saved.
This is guardrail #2 from ADR-0004, made concrete.

## Tool 2 — `check_job_exists`

Wraps `IJobIngestService.ExistsByUrlHashAsync` (proposed interface, not
yet implemented).

```json
{
  "name": "check_job_exists",
  "description": "Check whether a job posting URL has already been saved from a previous run, before spending effort fetching and parsing it.",
  "input_schema": {
    "type": "object",
    "properties": {
      "url": { "type": "string" }
    },
    "required": ["url"]
  }
}
```

**Returns:** `{ "exists": boolean }`

**Why it's a separate step before fetching:** lets Claude skip
`fetch_job_page`/parsing entirely for already-known jobs, which is both
cheaper (fewer tokens, fewer tool calls toward the iteration cap) and
lower-risk (nothing to hallucinate about a job it never reads).

## Tool 3 — `fetch_job_page`

Wraps `IJobLinksScraper.FetchHtmlAsync`. Plain C#, no AI inside the tool.

```json
{
  "name": "fetch_job_page",
  "description": "Fetch the raw page content for a single job posting URL, so you can read and extract its details yourself.",
  "input_schema": {
    "type": "object",
    "properties": {
      "url": { "type": "string" }
    },
    "required": ["url"]
  }
}
```

**Returns:** `{ "text": string }` — HTML with `<script>`/`<style>`/nav
chrome stripped server-side (plain HtmlAgilityPack cleanup, not AI) before
returning, to save tokens. Full markup is not needed once Claude is doing
the extraction itself.

**Guardrail hook:** rejects (returns a tool error) if `url` isn't in the
run's allow-list from `scrape_job_links`. Claude cannot fetch an arbitrary
URL it invents mid-conversation.

**This is where parsing actually happens** — not inside a tool, but in
Claude's own reasoning over the returned text, which then feeds the
extracted fields into `save_job` below. This keeps the "Claude parses the
job" requirement literal, and keeps the full extraction visible in the
transcript (guardrail #5) rather than hidden inside a nested tool call.

## Tool 4 — `save_job`

Wraps `IJobIngestService.CreateAsync` (proposed interface).

```json
{
  "name": "save_job",
  "description": "Persist a job posting you've extracted from a fetched page. Only call this for a URL you actually fetched with fetch_job_page in this conversation.",
  "input_schema": {
    "type": "object",
    "properties": {
      "url": { "type": "string" },
      "title": { "type": "string" },
      "company": { "type": "string" },
      "location": { "type": "string" },
      "salaryRaw": { "type": "string", "description": "Salary as stated on the page, unparsed. Empty string if not mentioned." },
      "descriptionRaw": { "type": "string" },
      "postedAt": { "type": "string", "format": "date", "description": "Omit the field entirely if the posting date isn't stated." }
    },
    "required": ["url", "title", "company", "descriptionRaw"]
  }
}
```

**Returns:** `{ "jobId": "guid", "saved": true }` on success, or a tool
error if the guardrail check fails.

**Guardrail hooks (both mandatory, both server-side, per ADR-0004 #2):**
1. `url` must be in the run's allow-list — rejected otherwise.
2. `UrlHash` is computed by C# from `url` itself. There is no `urlHash`
   input field, on purpose — nothing about the hash is ever accepted from
   the model.

## Tool 5 — `score_relevance`

Wraps `IJobMatchService` (proposed — the match-creation half of it).

```json
{
  "name": "score_relevance",
  "description": "Submit your relevance assessment of a saved job against the candidate's profile. You compute the score and reasoning yourself; this tool just records it.",
  "input_schema": {
    "type": "object",
    "properties": {
      "jobId": { "type": "string", "format": "uuid" },
      "score": { "type": "integer", "minimum": 0, "maximum": 100 },
      "reason": { "type": "string", "description": "One or two sentences." }
    },
    "required": ["jobId", "score", "reason"]
  }
}
```

**Returns:** `{ "matched": boolean, "thresholdApplied": 65 }`

**Guardrail hook (ADR-0004 #4):** the tool clamps `score` to [0, 100] and
compares it against `AnthropicSettings:RelevanceThreshold` **in C#**. A
`UserJobMatch` is created only if the numeric comparison passes — Claude's
own confidence in its text `reason` has no bearing on whether a match gets
written. If `score` is below threshold, the tool still returns
successfully (`matched: false`) — this is a normal outcome, not an error.

## Tool 6 — `send_digest_email`

Wraps `IEmailSender.SendJobDigestAsync` (not yet implemented — see note
below) + `IJobMatchService.GetUnnotifiedAsync` + `MarkAsNotifiedAsync`.

```json
{
  "name": "send_digest_email",
  "description": "Send the user an email digest of all newly matched jobs from this run, once you've finished scraping and scoring every active site. Call this exactly once, at the end.",
  "input_schema": {
    "type": "object",
    "properties": {},
    "required": []
  }
}
```

**Returns:** `{ "sent": true, "jobCount": 3 }`

Deliberately takes **no input** — Claude doesn't enumerate which matches
to send. The C# implementation fetches unnotified matches for this user
itself (`GetUnnotifiedAsync`), builds the email, sends it, then marks them
notified (`MarkAsNotifiedAsync`). This removes any chance of Claude
describing jobs in the email that don't match what's actually in the
database. **Blocked on `IEmailSender` implementation**, which we're
deferring per your earlier call — this tool's schema is final, but it
can't be wired up until that exists.

## Loop-level guardrails (not a tool, orchestrator-level)

- **Iteration cap (ADR-0004 #3):** the C# orchestrator running the loop
  enforces a hard max on total tool calls per run (exact number TBD —
  needs a config value, e.g. `WorkerSettings:MaxAgentToolCalls`; with <10
  sites and a handful of jobs each, something like 150–200 gives generous
  headroom without being effectively unlimited).
- **Transcript logging (ADR-0004 #5):** every tool call + arguments +
  result logged per run. Exact storage (structured log vs. a dedicated
  table) is an open question for the implementation step, not this design
  pass.

## Open questions before implementation

1. **Per-site job cap.** Nothing above limits how many links per site
   Claude tries to process — should there be a max (e.g. reuse/repurpose
   `WorkerSettings:MaxPagesPerSite`, currently unused by any implemented
   code) so one large site can't consume the whole iteration budget?
2. **`postedAt` parsing.** Job sites format dates wildly inconsistently
   ("2 days ago", "Posted July 2026", ISO dates). Should Claude attempt to
   normalize to a date, or should `postedAt` be freeform text and left as
   `null`/unparsed in `JobDto` if ambiguous, deferring normalization to a
   later pass?
3. **System prompt content.** Not designed yet — needs to state the goal,
   the guardrail expectations in plain language (e.g. "only save jobs from
   URLs you fetched with fetch_job_page"), and the stop condition
   (call `send_digest_email` once all sites are processed, then stop).
