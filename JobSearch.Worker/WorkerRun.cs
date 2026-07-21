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

    public WorkerRun(
        ILogger<WorkerRun> logger,
        IUserProfileService userProfileService)
    {
        _logger = logger;
        _userProfileService = userProfileService;
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
            return 1;
        }

        _logger.LogInformation("Resolved current user: {UserId}", userId);

        // TODO: load user profile (IUserProfileService) in parallel with
        //       loading active job sites (IJobSiteQueryService.GetAllActiveAsync)
        // TODO: scrape job links per site (IJobLinksScraper); skip jobs
        //       that already exist (dedup by UrlHash / JobSiteId+ExternalId)
        // TODO: match new jobs to the profile (IJobMatchService)
        // TODO: fetch unnotified matches, send email digest (IEmailSender),
        //       then mark them as notified (MarkAsNotifiedAsync)

        _logger.LogInformation(
            "Worker run finished at {time}", DateTimeOffset.UtcNow);
        return 0;
    }
}
