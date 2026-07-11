namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserPersistenceDto(
    Guid id,
    string email,
    string name,
    DateTime createdAt,
    bool isActive)
{
    public Guid Id { get; } = id;
    public string Email { get; } = email;
    public string Name { get; } = name;
    public DateTime CreatedAt { get; } = createdAt;
    public bool IsActive { get; } = isActive;
}
