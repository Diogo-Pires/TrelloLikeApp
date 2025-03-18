using Application.Task.DTOs;
using Application.Task.Interfaces;
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

public class TaskFunction(ITaskService taskService,
                          IExceptionHandler exceptionHandler,
                          IValidator<TaskEntityDTO> createValidator,
                          IValidator<TaskEntityDTO> updateValidator)
{
    const string ROUTE_NAME = "tasks";
    private readonly ITaskService _taskService = taskService;
    private readonly IExceptionHandler _exceptionHandler = exceptionHandler;
    private readonly IValidator<TaskEntityDTO> _createValidator = createValidator;
    private readonly IValidator<TaskEntityDTO> _updateValidator = updateValidator;

    /// <summary>
    /// Get all tasks.
    /// </summary>
    /// <returns><see cref="List<TaskDTO>"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET tasks
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    [OpenApiOperation(operationId: nameof(GetAllTasks), tags: [ROUTE_NAME])]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: UtilityConsts.APPJSON, bodyType: typeof(List<TaskEntityDTO>), Description = "Get all the tasks")]
    [FunctionName(nameof(GetAllTasks))]
    public async Task<IActionResult> GetAllTasks(
        [HttpTrigger(AuthorizationLevel.Anonymous, UtilityConsts.GET, Route = $"{ROUTE_NAME}")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var taskList = await _taskService.GetAllAsync(cancellationToken);
            return new OkObjectResult(taskList);
        }
        catch (Exception ex)
        {
            return _exceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Get a task by id.
    /// </summary>
    /// <param name="nameof(id)"></param>
    /// <returns><see cref="TaskEntityDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET tasks/id
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    [OpenApiOperation(operationId: nameof(GetTaskById), tags: [ROUTE_NAME])]
    [OpenApiParameter(name: nameof(id), In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "Task's id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: UtilityConsts.APPJSON, bodyType: typeof(TaskEntityDTO), Description = "Get a task by id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: UtilityConsts.APPJSON, bodyType: typeof(ErrorResponse), Description = "Model for errors")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Task was not found")]
    [FunctionName(nameof(GetTaskById))]
    public async Task<IActionResult> GetTaskById(
        [HttpTrigger(AuthorizationLevel.Anonymous, UtilityConsts.GET, Route = $"{ROUTE_NAME}/{{id}}")] HttpRequest req, 
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(Constants.VALIDATION_TASK_ID_NOT_EMPTY).Errors);
            }

            var task = await _taskService.GetByIdAsync(id, cancellationToken);
            if (task == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(task);
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
    /// Creates a task.
    /// </summary>
    /// <param name="nameof(req)"></param>
    /// <returns><see cref="TaskEntityDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// POST tasks/
    /// {
    /// "title": "Aprender Azure 41",
    /// "description": "Estudar Azure Functions 2",
    /// "isCompleted": false
    ///}
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="201">Created</response>
    /// <response code="400">Bad Request</response>
    [OpenApiOperation(operationId: nameof(CreateTask), tags: [ROUTE_NAME])]
    [OpenApiParameter(name: nameof(req), In = ParameterLocation.Path, Required = true, Type = typeof(TaskEntityDTO), Description = "A new task")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: UtilityConsts.APPJSON, bodyType: typeof(TaskEntityDTO), Description = "Creates a new task")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Provided task was wrongly formated")]
    [FunctionName(nameof(CreateTask))]
    public async Task<IActionResult> CreateTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, UtilityConsts.POST, Route = $"{ROUTE_NAME}")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var createTaskDto = JsonConvert.DeserializeObject<TaskEntityDTO>(requestBody);

            var validationResult = await _createValidator.ValidateAsync(createTaskDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(validationResult.Errors).Errors);
            }

            var createdTask = await _taskService.CreateAsync(createTaskDto, cancellationToken);
            if(createdTask.IsFailed)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(createdTask.Errors).Errors);
            }

            return new CreatedAtActionResult(
                nameof(GetTaskById),
                "Task",
                new { id = createdTask.Value.Id },
                createdTask
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

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="nameof(req)"></param>
    /// <returns><see cref="TaskEntityDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// PUT tasks/
    /// {
    /// "id": "fc7c69b1-27cb-4dd9-a633-45cce665a563",
    /// "title": "Aprender Azure 41",
    /// "description": "Estudar Azure Functions 2",
    /// "isCompleted": false
    ///}
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    [OpenApiOperation(operationId: nameof(UpdateTask), tags: [ROUTE_NAME])]
    [OpenApiParameter(name: nameof(req), In = ParameterLocation.Path, Required = true, Type = typeof(TaskEntityDTO), Description = "A task to be update")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: UtilityConsts.APPJSON, bodyType: typeof(TaskEntityDTO), Description = "Updates a task")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: UtilityConsts.APPJSON, bodyType: typeof(string), Description = "Provided task was wrongly formated")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Task was not found")]
    [FunctionName(nameof(UpdateTask))]
    public async Task<IActionResult> UpdateTask(
        [HttpTrigger(AuthorizationLevel.Function, UtilityConsts.PUT, Route = $"{ROUTE_NAME}")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var updateTaskDto = JsonConvert.DeserializeObject<TaskEntityDTO>(requestBody);

            var validationResult = await _updateValidator.ValidateAsync(updateTaskDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(validationResult.Errors).Errors);
            }

            var updatedTask = await _taskService.UpdateAsync(updateTaskDto, cancellationToken);
            if (updatedTask.IsFailed)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(updatedTask.Errors).Errors);
            }

            return new OkObjectResult(updatedTask.Value);
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
    /// Deletes a task.
    /// </summary>
    /// <param name="nameof(id)"></param>
    /// <returns><see cref="NoContentResult"/></returns>
    /// <remarks>
    /// Usage Example:
    /// DELETE tasks/id
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="204">No Content</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    [FunctionName(nameof(DeleteTask))]
    [OpenApiOperation(operationId: nameof(DeleteTask), tags: [ROUTE_NAME])]
    [OpenApiParameter(name: nameof(id), In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id of the task to be delete")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "Deletes a task")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: UtilityConsts.APPJSON, bodyType: typeof(ErrorResponse), Description = "Model for errors")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Task was not found")]
    public async Task<IActionResult> DeleteTask(
        [HttpTrigger(AuthorizationLevel.Function, UtilityConsts.DELETE, Route = $"{ROUTE_NAME}/{{id}}")] HttpRequest req, 
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(Constants.VALIDATION_TASK_ID_NOT_EMPTY).Errors);
            }

            var deleted = await _taskService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return new NotFoundResult();
            }

            return new NoContentResult();
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
    /// Assign a task to a user.
    /// </summary>
    /// <param name="nameof(id)"></param>
    /// <param name="nameof(email)"></param>
    /// <returns><see cref="TaskEntityDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// PATCH tasks/id/assign/email
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="404">Not Found</response>
    [OpenApiOperation(operationId: nameof(AssignedUserToATask), tags: [ROUTE_NAME])]
    [OpenApiParameter(name: nameof(taskId), In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The id of the task to be updated")]
    [OpenApiParameter(name: nameof(email), In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The user's email to be assigned to the task")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: UtilityConsts.APPJSON, bodyType: typeof(TaskEntityDTO), Description = "The updated task")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: UtilityConsts.APPJSON, bodyType: typeof(string), Description = "Provided values contains errors")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Task was not found")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "User was not found")]
    [FunctionName(nameof(AssignedUserToATask))]
    public async Task<IActionResult> AssignedUserToATask(
        [HttpTrigger(AuthorizationLevel.Anonymous, UtilityConsts.PATCH, Route = $"{ROUTE_NAME}/{{taskId}}/assign/{{email}}")] HttpRequest req,
        Guid taskId,
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            if (taskId == Guid.Empty)
            {
                return new BadRequestObjectResult(new ApiErrorResponse(Constants.VALIDATION_TASK_ID_NOT_EMPTY).Errors);
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return new BadRequestObjectResult(new ApiErrorResponse(Constants.VALIDATION_USER_EMAIL_NOT_EMPTY).Errors);
            }

            var task = await _taskService.AssignTaskToUserAsync(taskId, email, cancellationToken);
            if (task == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(task);
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