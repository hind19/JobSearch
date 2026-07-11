using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;

public class CvAnalysisResult(
    bool isSuccess,
    string? errorMessage,
    CandidateInfo candidate,
    List<UserSkillDto> skills,
    List<WorkExperienceDto> workExperience,
    List<string> detectedLanguages,
    List<string> desiredRoles,
    string claudeReadyProfile,
    List<ClarifyingQuestionDto> clarifyingQuestions)
{
    public bool IsSuccess { get; } = isSuccess;
    public string? ErrorMessage { get; } = errorMessage;
    public CandidateInfo Candidate { get; } = candidate;
    public List<UserSkillDto> Skills { get; } = skills;
    public List<WorkExperienceDto> WorkExperience { get; } = workExperience;
    public List<string> DetectedLanguages { get; } = detectedLanguages;
    public List<string> DesiredRoles { get; } = desiredRoles;
    public string ClaudeReadyProfile { get; } = claudeReadyProfile;
    public List<ClarifyingQuestionDto> ClarifyingQuestions { get; } = clarifyingQuestions;

    public static CvAnalysisResult Failure(string errorMessage) =>
        new(
            isSuccess: false,
            errorMessage: errorMessage,
            candidate: new CandidateInfo(null, null, null, null, null),
            skills: [],
            workExperience: [],
            detectedLanguages: [],
            desiredRoles: [],
            claudeReadyProfile: string.Empty,
            clarifyingQuestions: []);

    public static CvAnalysisResult WithQuestions(
        CvAnalysisResult source,
        List<ClarifyingQuestionDto> questions) =>
        new(
            isSuccess: source.IsSuccess,
            errorMessage: source.ErrorMessage,
            candidate: source.Candidate,
            skills: source.Skills,
            workExperience: source.WorkExperience,
            detectedLanguages: source.DetectedLanguages,
            desiredRoles: source.DesiredRoles,
            claudeReadyProfile: source.ClaudeReadyProfile,
            clarifyingQuestions: questions);
}

public class CandidateInfo(
    string? fullName,
    string? email,
    string? phone,
    string? location,
    string? summary)
{
    public string? FullName { get; } = fullName;
    public string? Email { get; } = email;
    public string? Phone { get; } = phone;
    public string? Location { get; } = location;
    public string? Summary { get; } = summary;
}

public class SkillDto(
    string skillName = "",
    ProficiencyLevel proficiencyLevel = default,
    int? yearsOfExperience = null,
    bool extractedByClaude = true)
{
    public string SkillName { get; } = skillName;
    public ProficiencyLevel ProficiencyLevel { get; } = proficiencyLevel;
    public int? YearsOfExperience { get; } = yearsOfExperience;
    public bool ExtractedByClaude { get; } = extractedByClaude;
}

public class WorkExperienceDto(
    string company,
    string role,
    DateOnly? startDate,
    DateOnly? endDate,
    string? description)
{
    public string Company { get; } = company;
    public string Role { get; } = role;
    public DateOnly? StartDate { get; } = startDate;
    public DateOnly? EndDate { get; } = endDate;
    public string? Description { get; } = description;
}
