namespace JobSearch.AI.CvParserService
{
    internal sealed class CvAnalysisRaw
    {
        public string? FullName { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? Location { get; init; }
        public string? Summary { get; init; }
        public List<string> DesiredRoles { get; init; } = [];
        public List<SkillRaw> Skills { get; init; } = [];
        public List<WorkExperienceRaw> WorkExperience { get; init; } = [];
        public List<string> DetectedLanguages { get; init; } = [];
    }
}
