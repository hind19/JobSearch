// JobSearch.AI/JobSearchAgentService/JobSearchAgentPrompts.cs
using System.Text;
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.AI.JobSearchAgentService;

internal static class JobSearchAgentPrompts
{
    // Static, reusable across runs — the variable parts (profile, sites)
    // go in the first user message instead, built by
    // BuildInitialUserMessage below.
    internal const string System = """
        You are helping a job-search assistant find and evaluate job
        postings for one candidate, using the tools provided. Work
        through every active job site given to you in the first message.

        For each job site:
        1. Call scrape_job_links to get its current posting URLs.
        2. For each URL, call check_job_exists first. If it already
           exists, skip it entirely — do not fetch or re-score it.
        3. For new URLs, call fetch_job_page to read the posting, then
           extract the job details yourself from what you read (title,
           company, location, salary if stated, full description,
           posting date if stated).
        4. Call save_job with what you extracted. Only ever call
           save_job with a URL you actually fetched with fetch_job_page
           in this conversation — never invent or guess a URL.
        5. Once saved, compare the job against the candidate profile
           given to you and call score_relevance with your own numeric
           assessment (0-100) and a short reason.

        When you've processed every active job site and every new job
        on each, stop — do not call any more tools. Do not fabricate
        job details you didn't actually read on a fetched page.
        """;

    // TODO (open question from worker-agent-tool-design.md): postedAt
    // normalization isn't specified beyond "omit if ambiguous" — revisit
    // if downstream sorting/display needs consistently parseable dates.

    internal static string BuildInitialUserMessage(
        UserProfileDto profile,
        List<JobSiteDto> activeSites)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Candidate profile:");
        sb.AppendLine(profile.ClaudeReadyProfile);
        sb.AppendLine();
        sb.AppendLine("Active job sites to process:");

        foreach (var site in activeSites)
            sb.AppendLine($"- jobSiteId: {site.Id}, name: {site.Name}, url: {site.BaseUrl}");

        return sb.ToString();
    }
}
