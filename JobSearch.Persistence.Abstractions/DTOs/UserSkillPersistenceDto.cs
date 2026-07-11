namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserSkillPersistenceDto(
    Guid id,
    Guid userId,
    string skillName,
    string proficiencyLevel,
    decimal? yearsOfExperience,
    bool extractedByClaude)
{
    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public string SkillName { get; } = skillName;
    public string ProficiencyLevel { get; } = proficiencyLevel;
    public decimal? YearsOfExperience { get; } = yearsOfExperience;
    public bool ExtractedByClaude { get; } = extractedByClaude;
}
