// Entities/User.cs
namespace JobSearch.Persistence.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public UserProfile? Profile { get; set; }
    public List<UserSkill> Skills { get; set; } = [];
    public List<UserJobMatch> JobMatches { get; set; } = [];
}