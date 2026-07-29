// ADR-0009 — pagination is enforced at the repository level (Skip/Take +
// a separate COUNT), not loaded-then-sliced in memory, so TotalCount
// must travel alongside the page of items.
namespace JobSearch.Persistence.Abstractions.DTOs;

public class RejectedJobsPagePersistenceDto(
    List<UserJobRejectionPersistenceDto> items,
    int totalCount)
{
    public List<UserJobRejectionPersistenceDto> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
}
