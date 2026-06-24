namespace JobSearch.AI.CvParserService
{
    internal sealed class WorkExperienceRaw
    {
        public string Company { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public string? StartDate { get; init; }
        public string? EndDate { get; init; }
        public string? Description { get; init; }
    }
}
