using Application.User.DTOs;
using Domain.Task.Enums;
using System.Text.Json.Serialization;

namespace Application.Task.DTOs;

public record TaskEntityDTO
{
    public Guid? Id { get; init; }

    public string Title { get; init; }

    public string Description { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskEntityStatus? Status { get; init; }

    [JsonIgnore]
    public DateTime? CreatedAt { get; init; }

    [JsonIgnore]
    public DateTime? CompletedAt { get; init; }

    public DateTime? Deadline { get; init; }

    public UserEntityDTO? User { get; init; }

    public TaskEntityDTO()
    {

    }

    public TaskEntityDTO(Guid id,
                   string title,
                   string description,
                   TaskEntityStatus? status,
                   DateTime? createdAt,
                   DateTime? completedAt,
                   DateTime? deadline,
                   UserEntityDTO? userDTO)
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

    public TaskEntityDTO(string title,
                   string description,
                   TaskEntityStatus? status,
                   DateTime? deadline,
                   UserEntityDTO? userDTO)
    {
        Title = title;
        Description = description;
        Status = status;
        Deadline = deadline;
        User = userDTO;
    }
}
