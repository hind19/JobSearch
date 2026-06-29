// Entities/UserSkill.cs
namespace JobSearch.Persistence.Entities;

public class UserSkill
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = "NotSpecified";
    public decimal? YearsOfExperience { get; set; }
    public bool ExtractedByClaude { get; set; }

    public User User { get; set; } = null!;
}