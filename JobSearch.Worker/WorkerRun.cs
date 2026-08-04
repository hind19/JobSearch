using JobSearch.Application.Abstractions.Interfaces;

namespace JobSearch.Worker;

// ADR-0001: single-run unit of work — no BackgroundService, no internal
// timer/loop. Program.cs resolves this once via DI, calls ExecuteAsync,
// and the process exits. Renamed from the BackgroundService-based
// "Worker" template class to make that explicit.
public class WorkerRun
{
    private readonly ILogger<WorkerRun> _logger;
    private readonly IUserProfileService _userProfileService;
    private readonly IJobSiteQueryService _jobSiteQueryService;
    private readonly IJobSearchAgent _jobSearchAgent;
    private readonly IJobMatchService _jobMatchService;
    private readonly IEmailSender _emailSender;

    public WorkerRun(
        ILogger<WorkerRun> logger,
        IUserProfileService userProfileService,
        IJobSiteQueryService jobSiteQueryService,
        IJobSearchAgent jobSearchAgent,
        IJobMatchService jobMatchService,
        IEmailSender emailSender)
    {
        _logger = logger;
        _userProfileService = userProfileService;
        _jobSiteQueryService = jobSiteQueryService;
        _jobSearchAgent = jobSearchAgent;
        _jobMatchService = jobMatchService;
        _emailSender = emailSender;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Worker run started at {time}", DateTimeOffset.UtcNow);

        // ADR-0002: resolve the target user as the most recently modified
        // user in the database. No login/bypass — replaces
        // JobSearch.WPF's LoginViewModel bypass flow for this headless
        // process.
        var userId = await _userProfileService.GetCurrentUserIdAsync(ct);

        if (userId is null)
        {
            // Hard fail per ADR-0002: unlike WPF, there is no UI here for
            // someone to create a profile if resolution comes back empty.
            _logger.LogError("No user found in the database. Aborting run.");
            // TODO: notify user by email (IEmailSender) — this is a
            //       user-facing configuration problem they should know
            //       about, not just a silent log entry.
            return 1;
        }

        _logger.LogInformation("Resolved current user: {UserId}", userId);

        // Per the pipeline diagram: profile load and active-site load run
        // in parallel, both gate the next step (scraping/matching).
        var profileTask = _userProfileService.GetProfileAsync(userId.Value, ct);
        var jobSitesTask = _jobSiteQueryService.GetAllActiveAsync(ct);

        await Task.WhenAll(profileTask, jobSitesTask);

        var profile = await profileTask;
        var jobSites = await jobSitesTask;

        if (profile is null)
        {
            // No saved profile yet (e.g. user created via CV analysis was
            // never completed/saved) — nothing to match jobs against.
            _logger.LogError(
                "User {UserId} has no saved profile. Aborting run.", userId);
            // TODO: notify user by email (IEmailSender) — profile missing
            //       is a user-facing configuration problem they should
            //       know about, not just a silent log entry.
            return 1;
        }

        _logger.LogInformation(
            "Loaded profile for {UserId} and {SiteCount} active job site(s).",
            userId, jobSites.Count);

        if (jobSites.Count == 0)
        {
            // Empty active-site list is a user configuration error (they
            // either never added a site or deactivated all of them) — not
            // a benign "nothing to do" case, so this aborts the run too.
            _logger.LogError(
                "User {UserId} has no active job sites configured. Aborting run.", userId);
            // TODO: notify user by email (IEmailSender) — same reasoning
            //       as the missing-profile case above.
            return 1;
        }

        // ADR-0004: scraping, parsing, and matching are delegated to the
        // Claude agent loop — Claude decides the per-site/per-job
        // sequence itself via tool calls (see worker-agent-tool-design.md
        // and JobSearchAgent). This single call replaces what would
        // otherwise be four separate deterministic pipeline steps.
        var agentResult = await _jobSearchAgent.RunAsync(
            userId.Value, profile, jobSites, ct);

        _logger.LogInformation(
            "Agent run finished: {ToolCallCount} tool call(s), " +
            "{JobsSaved} job(s) saved, {MatchesCreated} match(es) created, " +
            "completed={Completed}.",
            agentResult.ToolCallCount, agentResult.JobsSaved,
            agentResult.MatchesCreated, agentResult.Completed);

        if (!agentResult.Completed)
        {
            // Hit the iteration cap (ADR-0004 guardrail #3) without a
            // final answer from Claude — not a hard failure (whatever it
            // did save/match up to that point is still valid and
            // committed), but worth surfacing loudly rather than logging
            // at Information level.
            _logger.LogWarning(
                "Agent run for user {UserId} did not complete within the " +
                "tool-call cap — results may be partial.", userId);
        }

        // send_digest_email isn't a registered agent tool (ADR-0005 §6 —
        // sending is a deterministic, irreversible side effect, not left
        // to the model's discretion).
        var unnotified = await _jobMatchService.GetUnnotifiedAsync(userId.Value, ct);

        if (unnotified.Count > 0)
        {
            var user = await _userProfileService.GetUserAsync(userId.Value, ct);

            if (user is null)
            {
                // Shouldn't happen — userId was just resolved from this
                // same Users table — but fail loudly rather than send to
                // a blank address if it somehow does.
                _logger.LogError(
                    "User {UserId} disappeared between resolution and " +
                    "digest send. Skipping email.", userId);
            }
            else
            {
                var sendResult = await _emailSender.SendJobDigestAsync(
                    userId.Value, user.Email, unnotified, ct);

                if (sendResult.Sent)
                {
                    // ADR-0005 §5: only mark notified on actual success —
                    // a failed send (even after all retries) leaves these
                    // unnotified so the next run picks them up again.
                    var jobIds = unnotified.Select(m => m.JobId).ToList();
                    await _jobMatchService.MarkAsNotifiedAsync(userId.Value, jobIds, ct);

                    _logger.LogInformation(
                        "Sent digest with {Count} match(es) to {Email}.",
                        unnotified.Count, user.Email);
                }
                else
                {
                    _logger.LogWarning(
                        "Digest send failed for user {UserId}: {Error}. " +
                        "{Count} match(es) remain unnotified for the next run.",
                        userId, sendResult.ErrorMessage, unnotified.Count);
                }
            }
        }

        _logger.LogInformation(
            "Worker run finished at {time}", DateTimeOffset.UtcNow);
        return 0;
    }
}
