namespace Application.User.DTOs;

public record UserEntityDTO
{
    public string Id { get; init; }
    public string Name { get; init; }

    public UserEntityDTO()
    {
            
    }

    public UserEntityDTO(string name, string email)
    {
        Id = email;
        Name = name;
    }
}