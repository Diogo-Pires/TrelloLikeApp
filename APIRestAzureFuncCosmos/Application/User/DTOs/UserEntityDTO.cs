namespace Application.User.DTOs;

public record UserEntityDTO
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string GoogleId { get; init; }

    public UserEntityDTO()
    {
    }

    public UserEntityDTO(string name, string email, string googleId)
    {
        Id = email;
        Name = name;
        GoogleId = googleId;
    }
}