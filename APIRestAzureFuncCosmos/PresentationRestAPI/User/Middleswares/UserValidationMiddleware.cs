using Application.User.DTOs;
using Application.User.Interfaces;
using PresentationRestAPI.User.Interfaces;
using System.Security.Claims;

namespace PresentationRestAPI.User.Middleswares;

public class UserValidationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async System.Threading.Tasks.Task Invoke(HttpContext context,
                                                    IUserService userService,
                                                    IUserCreatorValidator createValidator)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var name = user.FindFirst("name")?.Value ?? email;
        var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(email) || 
            string.IsNullOrWhiteSpace(name) || 
            string.IsNullOrWhiteSpace(googleId))
        {
            await _next(context);
            return;
        }

        if (await userService.GetByEmailAsync(email, CancellationToken.None) != null)
        {
            await _next(context);
            return;
        }

        var userDTO = new UserEntityDTO(name, email, googleId);
        var validationResult = await createValidator.ValidateAsync(userDTO, CancellationToken.None);
        if (!validationResult.IsValid)
        {
            await _next(context);
            return;
        }

        var createdUser = await userService.CreateAsync(userDTO, CancellationToken.None);
        if (createdUser.IsFailed)
        {
            await _next(context);
            return;
        }

        await _next(context);
    }
}