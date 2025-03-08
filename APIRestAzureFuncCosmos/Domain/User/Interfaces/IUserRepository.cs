namespace Domain.User.Interfaces;

public interface IUserRepository
{
    Task<List<UserEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken);
}