using Application.User.DTOs;
using Application.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationRestAPI.DTOs;
using Shared.Consts;

namespace PresentationRestAPI.User;

[ApiController]
[Authorize]
[Produces(UtilityConsts.APPJSON)]
[Route("[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Get all users.
    /// </summary>
    /// <returns><see cref="List<UserDTO>"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET users
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, Type = typeof(List<UserEntityDTO>))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadRequestObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var userList = await _userService.GetAllAsync(cancellationToken);
        return new OkObjectResult(userList);
    }

    /// <summary>
    /// Get a user by email.
    /// </summary>
    /// <param name="nameof(email)"></param>
    /// <returns><see cref="UserEntityDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET user/email
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{email}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, Type = typeof(UserEntityDTO))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadRequestObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, Type = typeof(NotFoundObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new BadRequestObjectResult(new ApiErrorResponse(Constants.VALIDATION_USER_EMAIL_NOT_EMPTY).Errors);
            }

            var user = await _userService.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(user);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new ApiErrorResponse(ex.Message).Errors);
        }
    }

    /// <summary>
    /// Deletes all users related cache.
    /// </summary>
    /// <returns><see cref="NoContentResult"/></returns>
    /// <remarks>
    /// Usage Example:
    /// DELETE users/
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="204">No Content</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent, Type = typeof(NoContentResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearAllCaches(
        CancellationToken cancellationToken)
    {
        await _userService.DeleteAllCacheAsync(cancellationToken);
        return new NoContentResult();
    }
}