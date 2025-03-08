using Application.User.DTOs;
using Domain.User;

namespace Application.User.Mappers;

public static class UserMapper
{
    public static UserEntity ToEntity(UserEntityDTO dto) =>
        new(dto.Name.Trim(), dto.Id.Trim());

    public static UserEntityDTO ToDTO(UserEntity entity) =>
        new(entity.Name, entity.Id);
}