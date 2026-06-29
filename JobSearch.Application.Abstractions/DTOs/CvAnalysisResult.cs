using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;

public class CvAnalysisResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public CandidateInfo Candidate { get; }
    public List<UserSkillDto> Skills { get; }
    public List<WorkExperienceDto> WorkExperience { get; }
    public List<string> DetectedLanguages { get; }
    public List<string> DesiredRoles { get; }
    public string ClaudeReadyProfile { get; }
    public List<ClarifyingQuestionDto> ClarifyingQuestions { get; }

    public CvAnalysisResult(
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
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Candidate = candidate;
        Skills = skills;
        WorkExperience = workExperience;
        DetectedLanguages = detectedLanguages;
        DesiredRoles = desiredRoles;
        ClaudeReadyProfile = claudeReadyProfile;
        ClarifyingQuestions = clarifyingQuestions;
    }

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
        clarifyingQuestions: questions
    );
}

public class CandidateInfo
{
    public string? FullName { get; }
    public string? Email { get; }
    public string? Phone { get; }
    public string? Location { get; }
    public string? Summary { get; }

    public CandidateInfo(
        string? fullName,
        string? email,
        string? phone,
        string? location,
        string? summary)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
        Location = location;
        Summary = summary;
    }
}

public class SkillDto
{
    public string SkillName { get; init; } = string.Empty;
    public ProficiencyLevel ProficiencyLevel { get; init; }
    public int? YearsOfExperience { get; init; }
    public bool ExtractedByClaude { get; init; } = true;
}

public class WorkExperienceDto
{
    public string Company { get; }
    public string Role { get; }
    public DateOnly? StartDate { get; }
    public DateOnly? EndDate { get; }
    public string? Description { get; }

    public WorkExperienceDto(
        string company,
        string role,
        DateOnly? startDate,
        DateOnly? endDate,
        string? description)
    {
        Company = company;
        Role = role;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
    }
}