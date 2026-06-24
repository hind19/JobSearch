using JobSearch.Application.Abstractions.DTOs;
using JobSearch.Application.Abstractions.Enums;

public class CvAnalysisResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public CandidateInfo Candidate { get; init; } = new();
    public List<SkillDto> Skills { get; init; } = [];
    public List<WorkExperienceDto> WorkExperience { get; init; } = [];
    public List<string> DetectedLanguages { get; init; } = [];
    public List<string> DesiredRoles { get; init; } = [];
    public string ClaudeReadyProfile { get; init; } = string.Empty;
    public List<ClarifyingQuestionDto> ClarifyingQuestions { get; set; } = [];
}

public class CandidateInfo
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Location { get; init; }
    public string? Summary { get; init; }
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
    public string Company { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
}