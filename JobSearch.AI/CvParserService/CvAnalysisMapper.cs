using JobSearch.AI.CvParserService;
using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.AI.Mapping;

internal static class CvAnalysisMapper
{
    internal static CvAnalysisResult ToResult(CvAnalysisRaw raw) =>
        new(
            isSuccess: true,
            errorMessage: null,
            candidate: ToCandidate(raw),
            skills: ToSkills(raw.Skills),
            workExperience: ToWorkExperience(raw.WorkExperience),
            detectedLanguages: raw.DetectedLanguages,
            desiredRoles: raw.DesiredRoles,
            claudeReadyProfile: BuildClaudeReadyProfile(raw),
            clarifyingQuestions: []
        );

    private static CandidateInfo ToCandidate(CvAnalysisRaw raw) =>
        new(
            fullName: raw.FullName,
            email: raw.Email,
            phone: raw.Phone,
            location: raw.Location,
            summary: raw.Summary
        );

    private static List<UserSkillDto> ToSkills(
        List<SkillRaw> skills) =>
        skills
            .Select(ToSkillDto)
            .ToList();

    private static UserSkillDto ToSkillDto(SkillRaw raw) =>
        new(
            id: Guid.NewGuid(),
            userId: Guid.Empty,
            skillName: raw.SkillName,
            proficiencyLevel: ParseProficiency(raw.ProficiencyLevel),
            yearsOfExperience: raw.YearsOfExperience,
            extractedByClaude: true
        );

    private static List<WorkExperienceDto> ToWorkExperience(
        List<WorkExperienceRaw> workExperience) =>
        workExperience
            .Select(ToWorkExperienceDto)
            .ToList();

    private static WorkExperienceDto ToWorkExperienceDto(
        WorkExperienceRaw raw) =>
        new(
            company: raw.Company,
            role: raw.Role,
            startDate: ParseDate(raw.StartDate),
            endDate: ParseDate(raw.EndDate),
            description: raw.Description
        );

    private static ProficiencyLevel ParseProficiency(string value) =>
        Enum.TryParse<ProficiencyLevel>(
            value,
            ignoreCase: true,
            out var result)
                ? result
                : ProficiencyLevel.NotSpecified;

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateOnly.TryParseExact(
            value,
            "yyyy-MM",
            out var date)
                ? date
                : null;
    }

    private static string BuildClaudeReadyProfile(CvAnalysisRaw raw)
    {
        var skills = raw.Skills
            .Select(s =>
                $"{s.SkillName} ({s.ProficiencyLevel}" +
                (s.YearsOfExperience.HasValue
                    ? $", {s.YearsOfExperience} yrs"
                    : string.Empty) +
                ")")
            .ToList();

        var roles = raw.DesiredRoles.Count != 0
            ? string.Join(", ", raw.DesiredRoles)
            : "not specified";

        var languages = raw.DetectedLanguages.Count != 0
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