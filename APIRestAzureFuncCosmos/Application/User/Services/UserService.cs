using Application.User.DTOs;
using Application.User.Interfaces;
using Application.User.Mappers;
using Domain.User.Interfaces;
using FluentResults;
using Infrastructure.Cache;
using Infrastructure.Cache.Interfaces;

namespace Application.User.Services;

public class UserService(IUserRepository userRepository,
                         IHybridCacheService hybridCacheService) : BaseHybridCacheService, IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHybridCacheService _cacheService = hybridCacheService;

    protected override string CacheKey { get => "user:"; }

    public async Task<List<UserEntityDTO>> GetAllAsync(CancellationToken cancellationToken)
    {
        var cachekey = $"{CacheKey}{BASE_CACHEKEY_ALL}";
        return await _cacheService
            .GetOrSetAsync(cachekey, async () =>
                (await _userRepository.GetAllAsync(cancellationToken))
                        .Select(UserMapper.ToDTO)
                        .ToList()
            ) ?? [];
    }

    public async Task<UserEntityDTO?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        email = email.Trim();
        var cachekey = $"{CacheKey}{email}";

        var user = await _cacheService
            .GetOrSetAsync(cachekey, async () =>
                await _userRepository.GetByEmailAsync(email, cancellationToken)
            );

        if (user == null)
        {
            return null;
        }

        return UserMapper.ToDTO(user);
    }

    public async Task<Result<UserEntityDTO>> CreateAsync(UserEntityDTO createUserDto, CancellationToken cancellationToken)
    {
        var userEntity = UserMapper.ToEntity(createUserDto);
        var createdUser = await _userRepository.AddAsync(userEntity, cancellationToken);

        await ClearAllRequestFromCacheAsync(_cacheService);

        return Result.Ok(UserMapper.ToDTO(createdUser));
    }
}