// DTOs/UserPersistenceDto.cs
namespace JobSearch.Persistence.Abstractions.DTOs;

public class UserPersistenceDto
{
    public Guid Id { get; }
    public string Email { get; }
    public string Name { get; }
    public DateTime CreatedAt { get; }
    public bool IsActive { get; }

    public UserPersistenceDto(
        Guid id,
        string email,
        string name,
        DateTime createdAt,
        bool isActive)
    {
        Id = id;
        Email = email;
        Name = name;
        CreatedAt = createdAt;
        IsActive = isActive;
    }
}