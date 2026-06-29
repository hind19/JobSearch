// DTOs/UserSkillPersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserSkillPersistenceDto
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string SkillName { get; }
    public string ProficiencyLevel { get; }
    public decimal? YearsOfExperience { get; }
    public bool ExtractedByClaude { get; }

    public UserSkillPersistenceDto(
        Guid id,
        Guid userId,
        string skillName,
        string proficiencyLevel,
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