// Entities/User.cs
namespace JobSearch.Persistence.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    // ADR-0002: bumped by UserRepository on every Create/Update; used to
    // resolve "most recently modified user" for the headless Worker.
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }

    public UserProfile? Profile { get; set; }
    public List<UserSkill> Skills { get; set; } = [];
    public List<UserJobMatch> JobMatches { get; set; } = [];
    public List<SentEmail> SentEmails { get; set; } = [];
}