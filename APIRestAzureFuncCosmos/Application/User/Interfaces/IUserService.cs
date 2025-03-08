using Application.User.DTOs;
using FluentResults;

namespace Application.User.Interfaces;

public interface IUserService
{
    Task<List<UserEntityDTO>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserEntityDTO?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Result<UserEntityDTO>> CreateAsync(UserEntityDTO createUserDto, CancellationToken cancellationToken);
}