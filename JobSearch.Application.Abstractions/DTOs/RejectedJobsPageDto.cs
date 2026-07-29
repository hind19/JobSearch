// ADR-0009
namespace JobSearch.Application.Abstractions.DTOs;

public class RejectedJobsPageDto(
    List<RejectedJobDto> items,
    int totalCount,
    int page,
    int pageSize)
{
    public List<RejectedJobDto> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;

    public int TotalPages =>
        TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
