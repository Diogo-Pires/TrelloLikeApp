using Application.User.DTOs;
using Application.User.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PresentationRestAPI.User.Interfaces;
using Shared.Consts;

namespace PresentationRestAPI.User;

[ApiController]
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
        try
        {
            var userList = await _userService.GetAllAsync(cancellationToken);
            return new OkObjectResult(userList);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
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
                return new BadRequestObjectResult(new { Error = Constants.VALIDATION_USER_EMAIL_NOT_EMPTY });
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
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Creates an user.
    /// </summary>
    /// <param name="nameof(req)"></param>
    /// <returns><see cref="UserEntityDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// POST user/
    /// {
    /// "name": "Diogo",
    /// "email": "diogo@domain.com"
    /// }
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="201">Created</response>
    /// <response code="400">Bad Request</response>
    [HttpPost]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, Type = typeof(CreatedAtActionResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadHttpRequestException))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser(UserEntityDTO userDTO,
                                                [FromServices]IUserCreatorValidator createValidator,
                                                CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await createValidator.ValidateAsync(userDTO, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors);
            }

            var createdUser = await _userService.CreateAsync(userDTO, cancellationToken);
            if (createdUser.IsFailed)
            {
                return new BadRequestObjectResult(new { Errors = createdUser.Errors.Select(e => e.Message) });
            }

            return new CreatedAtActionResult(
                nameof(GetUserByEmail),
                "User",
                new { createdUser.Value.Id },
                createdUser
            );
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }
}