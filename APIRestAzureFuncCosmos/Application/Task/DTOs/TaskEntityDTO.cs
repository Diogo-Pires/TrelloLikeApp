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

    public DateTime? CreatedAt { get; init; }

    [JsonIgnore]
    public DateTime? CompletedAt { get; init; }

    public DateTime? Deadline { get; init; }

    public string? AssignedUserEmail { get; init; }

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
                   string assignedUserEmail)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
        Deadline = deadline;
        AssignedUserEmail = assignedUserEmail;
    }

    public TaskEntityDTO(string title,
                   string description,
                   TaskEntityStatus? status,
                   DateTime? deadline,
                   string assignedUserEmail)
    {
        Title = title;
        Description = description;
        Status = status;
        Deadline = deadline;
        AssignedUserEmail = assignedUserEmail;
    }
}
