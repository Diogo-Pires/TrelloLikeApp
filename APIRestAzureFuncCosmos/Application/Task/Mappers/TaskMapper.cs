using Application.Task.DTOs;
using Application.User.Mappers;
using Domain.Task;
using Shared.Interfaces;

namespace Application.Task.Mappers;

public static class TaskMapper
{
    public static TaskEntity ToEntity(TaskEntityDTO dto, IDateTimeProvider dateTimeProvider) =>
        new(dto.Title.Trim(),
            dto.Description.Trim(),
            dto.Deadline,
            dto.Status,
            dto.AssignedUserEmail,
            dateTimeProvider);

    public static TaskEntityDTO ToDTO(TaskEntity entity) =>
        new(entity.Id,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.CreatedAt,
            entity.CompletedAt,
            entity.Deadline,
            entity.AssignedUserEmail);
}