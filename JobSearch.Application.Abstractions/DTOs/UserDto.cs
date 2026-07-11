namespace JobSearch.Application.Abstractions.DTOs;

public class UserDto(
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
