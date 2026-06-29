using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.Application.Abstractions.DTOs;

public class UserSkillDto
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string SkillName { get; }
    public ProficiencyLevel ProficiencyLevel { get; }
    public decimal? YearsOfExperience { get; }
    public bool ExtractedByClaude { get; }

    public UserSkillDto(
        Guid id,
        Guid userId,
        string skillName,
        ProficiencyLevel proficiencyLevel,
        decimal? yearsOfExperience,
        bool extractedByClaude)
    {
        Id = id;
        UserId = userId;
        SkillName = skillName;
        ProficiencyLevel = proficiencyLevel;
        YearsOfExperience = yearsOfExperience;
        ExtractedByClaude = extractedByClaude;
    }
}