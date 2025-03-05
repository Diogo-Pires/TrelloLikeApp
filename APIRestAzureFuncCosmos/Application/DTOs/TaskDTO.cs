
using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.DTOs;

public record TaskDTO
{
    public Guid? Id { get; init; }
    
    public string Title { get; init; }
    
    public string Description { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskItemStatus? Status { get; init; }

    [JsonIgnore]
    public DateTime? CreatedAt { get; init; }

    [JsonIgnore]
    public DateTime? CompletedAt { get; init; }
    
    public DateTime? Deadline { get; init; }
    
    public UserDTO? User { get; init; }

    public TaskDTO()
    {
            
    }

    public TaskDTO(Guid id,
                   string title,
                   string description,
                   TaskItemStatus? status,
                   DateTime? createdAt,
                   DateTime? completedAt,
                   DateTime? deadline,
                   UserDTO? userDTO)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
        Deadline = deadline;
        User = userDTO;
    }

    public TaskDTO(string title,
                   string description,
                   TaskItemStatus? status,
                   DateTime? deadline,
                   UserDTO? userDTO)
    {
        Title = title;
        Description = description;
        Status = status;
        Deadline = deadline;
        User = userDTO;
    }
}
