namespace JobSearch.AI.CvParserService
{
    internal sealed class SkillRaw
    {
        public string SkillName { get; init; } = string.Empty;
        public string ProficiencyLevel { get; init; } = "NotSpecified";
        public decimal? YearsOfExperience { get; init; }
    }
}
