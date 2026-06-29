namespace JobSearch.Application.Abstractions.DTOs;

public class UserDto
{
    public Guid Id { get; }
    public string Email { get; }
    public string Name { get; }
    public DateTime CreatedAt { get; }
    public bool IsActive { get; }

    public UserDto(
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