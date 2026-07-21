// JobSearch.Application.Abstractions/Interfaces/IJobSiteQueryService.cs
using JobSearch.Application.Abstractions.DTOs;

namespace JobSearch.Application.Abstractions.Interfaces;

// Read-only slice of job-site access for headless callers (JobSearch.Worker).
// Split out from IJobSiteService per ISP: Worker only ever needs the list
// of active sites to scrape — it must not depend on the WPF management
// surface (Create/Update/Delete/SetActive/ValidateConfig), which it never
// calls and has no business calling from an unattended process.
public interface IJobSiteQueryService
{
    Task<List<JobSiteDto>> GetAllActiveAsync(
        CancellationToken ct = default);
}
