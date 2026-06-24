using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.AI.CvParserService
{
    internal static class CvAnalysisMapper
    {
        internal static CvAnalysisResult ToResult(CvAnalysisRaw raw) => new()
        {
            IsSuccess = true,
            Candidate = new CandidateInfo
            {
                FullName = raw.FullName,
                Email = raw.Email,
                Phone = raw.Phone,
                Location = raw.Location,
                Summary = raw.Summary
            },
            Skills = raw.Skills
                .Select(ToSkillDto)
                .ToList(),
            WorkExperience = raw.WorkExperience
                .Select(ToWorkExperienceDto)
                .ToList(),
            DetectedLanguages = raw.DetectedLanguages,
            DesiredRoles = raw.DesiredRoles,
            ClaudeReadyProfile = BuildClaudeReadyProfile(raw)
        };

        private static SkillDto ToSkillDto(SkillRaw s) => new()
        {
            SkillName = s.SkillName,
            ProficiencyLevel = ParseProficiency(s.ProficiencyLevel),
            YearsOfExperience = s.YearsOfExperience.HasValue ? (int)Math.Round(s.YearsOfExperience.Value) : null,
            ExtractedByClaude = true
        };

        private static WorkExperienceDto ToWorkExperienceDto(
            WorkExperienceRaw w) => new()
            {
                Company = w.Company,
                Role = w.Role,
                StartDate = ParseDate(w.StartDate),
                EndDate = ParseDate(w.EndDate),
                Description = w.Description
            };

        private static ProficiencyLevel ParseProficiency(string value) =>
            Enum.TryParse<ProficiencyLevel>(value, ignoreCase: true, out var result)
                ? result
                : ProficiencyLevel.NotSpecified;

        private static DateOnly? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            return DateOnly.TryParseExact(
                value, "yyyy-MM",
                out var date) ? date : null;
        }

        private static string BuildClaudeReadyProfile(CvAnalysisRaw raw)
        {
            var skills = raw.Skills
                .Select(s => $"{s.SkillName} ({s.ProficiencyLevel}" +
                             (s.YearsOfExperience.HasValue
                                 ? $", {s.YearsOfExperience} yrs"
                                 : string.Empty) + ")")
                .ToList();

            var roles = raw.DesiredRoles.Any()
                ? string.Join(", ", raw.DesiredRoles)
                : "not specified";

            var languages = raw.DetectedLanguages.Any()
                ? string.Join(", ", raw.DetectedLanguages)
                : "not specified";

            return $"""
            Candidate: {raw.FullName ?? "Unknown"}
            Location: {raw.Location ?? "not specified"}
            Desired roles: {roles}
            Languages: {languages}
            Skills: {string.Join(", ", skills)}
            """;
        }
    }

}
