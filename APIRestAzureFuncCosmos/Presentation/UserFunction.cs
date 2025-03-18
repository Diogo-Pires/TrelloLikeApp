using Application.User.DTOs;
using Application.User.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Presentation.DTOs;
using Presentation.Interfaces;
using Shared.Consts;
using Shared.Validations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Presentation;

public class UserFunction(IUserService userService,
                          IValidator<UserEntityDTO> createValidator, 
                          IExceptionHandler exceptionHandler)
{
    const string ROUTE_NAME = "users";
    private readonly IUserService _userService = userService;
    private readonly IValidator<UserEntityDTO> _createValidator = createValidator;
    private readonly IExceptionHandler _exceptionHandler = exceptionHandler;

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
    [OpenApiOperation(operationId: nameof(GetAllUsers), tags: [ROUTE_NAME])]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: UtilityConsts.APPJSON, bodyType: typeof(List<UserEntityDTO>), Description = "Get all the users")]
    [FunctionName(nameof(GetAllUsers))]
    public async Task<IActionResult> GetAllUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, UtilityConsts.GET, Route = $"{ROUTE_NAME}")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var userList = await _userService.GetAllAsync(cancellationToken);
            return new OkObjectResult(userList);
        }
        catch (Exception ex)
        {
            return _exceptionHandler.HandleException(ex);
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
    [OpenApiOperation(operationId: nameof(GetUserByEmail), tags: [ROUTE_NAME])]
    [OpenApiParameter(name: nameof(email), In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "User's email")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: UtilityConsts.APPJSON, bodyType: typeof(UserEntityDTO), Description = "Get a user by email")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: UtilityConsts.APPJSON, bodyType: typeof(ErrorResponse), Description = "Model for errors")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "User was not found")]
    [FunctionName(nameof(GetUserByEmail))]
    public async Task<IActionResult> GetUserByEmail(
        [HttpTrigger(AuthorizationLevel.Anonymous, UtilityConsts.GET, Route = $"{ROUTE_NAME}/{{email}}")] HttpRequest req,
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
        catch (Exception ex)
        {
            return _exceptionHandler.HandleException(ex);
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
    [OpenApiOperation(operationId: nameof(CreateUser), tags: [ROUTE_NAME])]
    [OpenApiParameter(name: nameof(req), In = ParameterLocation.Path, Required = true, Type = typeof(UserEntityDTO), Description = "A new user")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: UtilityConsts.APPJSON, bodyType: typeof(UserEntityDTO), Description = "Creates a new user")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Provided user was wrongly formated")]
    [FunctionName(nameof(CreateUser))]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, UtilityConsts.POST, Route = $"{ROUTE_NAME}")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var createUserDto = JsonConvert.DeserializeObject<UserEntityDTO>(requestBody);

            var validationResult = await _createValidator.ValidateAsync(createUserDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(validationResult.Errors).Errors);
            }

            var createdUser = await _userService.CreateAsync(createUserDto, cancellationToken);
            if(createdUser.IsFailed)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(createdUser.Errors).Errors);
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
            return new BadRequestObjectResult(new ApiErrorResponse(ex.Message).Errors);
        }
        catch (Exception ex)
        {
            return _exceptionHandler.HandleException(ex);
        }
    }
}