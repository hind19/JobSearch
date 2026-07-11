using JobSearch.Application.Abstractions.Enums;

namespace JobSearch.Application.Abstractions.DTOs;

public class UserSkillDto(
    Guid id,
    Guid userId,
    string skillName,
    ProficiencyLevel proficiencyLevel,
    decimal? yearsOfExperience,
    bool extractedByClaude)
{
    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public string SkillName { get; } = skillName;
    public ProficiencyLevel ProficiencyLevel { get; } = proficiencyLevel;
    public decimal? YearsOfExperience { get; } = yearsOfExperience;
    public bool ExtractedByClaude { get; } = extractedByClaude;
}
