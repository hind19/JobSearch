using JobSearch.Persistence.Abstractions;
using JobSearch.Persistence.Abstractions.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JobSearch.Persistence.Repositories;

public class JobStatisticsRepository : IJobStatisticsRepository
{
    private readonly AppDbContext _context;

    public JobStatisticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobSiteStatisticsPersistenceDto>> GetStatisticsAsync(
        CancellationToken ct = default)
    {
        // Three separate queries, combined in memory, rather than one
        // LINQ query with nested aggregation over navigation collections
        // (js.Jobs.SelectMany(j => j.UserJobMatches).Average(...)) —
        // the latter risks hitting EF Core/SQLite translation limits for
        // doubly-nested aggregates inside a Select projection. This is
        // more verbose but reliably translates to SQL at each step.

        var jobsScrapedBySite = await _context.Jobs
            .GroupBy(j => j.JobSiteId)
            .Select(g => new { JobSiteId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var matchStatsBySite = await _context.UserJobMatches
            .Join(
                _context.Jobs,
                match => match.JobId,
                job => job.Id,
                (match, job) => new { match, job.JobSiteId })
            .GroupBy(x => x.JobSiteId)
            .Select(g => new
            {
                JobSiteId = g.Key,
                MatchesCount = g.Count(),
                AverageScore = g.Average(x => (decimal?)x.match.RelevanceScore),
                MostRecentMatchAt = g.Max(x => (DateTime?)x.match.FoundInRunAt)
            })
            .ToListAsync(ct);

        var sites = await _context.JobSites
            .AsNoTracking()
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        var jobsScrapedById = jobsScrapedBySite
            .ToDictionary(x => x.JobSiteId, x => x.Count);

        var matchStatsById = matchStatsBySite
            .ToDictionary(x => x.JobSiteId);

        // LEFT JOIN semantics: iterate all sites, not just ones with
        // jobs/matches, so a site with zero of either still appears with
        // 0/null rather than being silently excluded (ADR-0006).
        return sites
            .Select(s =>
            {
                jobsScrapedById.TryGetValue(s.Id, out var scrapedCount);
                matchStatsById.TryGetValue(s.Id, out var matchStats);

                return new JobSiteStatisticsPersistenceDto(
                    jobSiteId: s.Id,
                    jobSiteName: s.Name,
                    jobsScrapedCount: scrapedCount,
                    matchesCount: matchStats?.MatchesCount ?? 0,
                    averageRelevanceScore: matchStats?.AverageScore,
                    mostRecentMatchAt: matchStats?.MostRecentMatchAt);
            })
            .ToList();
    }
}
