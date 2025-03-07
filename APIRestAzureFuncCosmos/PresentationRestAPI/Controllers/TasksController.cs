using Application.DTOs;
using Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Consts;

namespace PresentationRestAPI.Controllers;

[ApiController]
[Produces(UtilityConsts.APPJSON)]
[Route("[controller]")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;

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
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, Type = typeof(List<TaskDTO>))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadRequestObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTasks(CancellationToken cancellationToken)
    {
        try
        {
            var taskList = await _taskService.GetAllAsync(cancellationToken);
            return new OkObjectResult(taskList);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get a task by id.
    /// </summary>
    /// <param name="nameof(id)"></param>
    /// <returns><see cref="TaskDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// GET task/id
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, Type = typeof(TaskDTO))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadRequestObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, Type = typeof(NotFoundObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTaskById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(new { Error = Constants.VALIDATION_TASK_ID_NOT_EMPTY });
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
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a task.
    /// </summary>
    /// <param name="nameof(req)"></param>
    /// <returns><see cref="TaskDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// POST task/
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
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, Type = typeof(TaskDTO))]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, Type = typeof(CreatedAtActionResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadHttpRequestException))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTask(TaskDTO taskDTO,
                                                [FromServices]IValidator<TaskDTO> createValidator,
                                                CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await createValidator.ValidateAsync(taskDTO, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors);
            }

            var createdTask = await _taskService.CreateAsync(taskDTO, cancellationToken);
            if (createdTask.IsFailed)
            {
                return new BadRequestObjectResult(new { Errors = createdTask.Errors.Select(e => e.Message) });
            }

            return new CreatedAtActionResult(
                nameof(GetTaskById),
                "Tasks",
                new { id = createdTask.Value.Id },
                createdTask
            );
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="nameof(req)"></param>
    /// <returns><see cref="TaskDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// PUT task/
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
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, Type = typeof(TaskDTO))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadHttpRequestException))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, Type = typeof(NotFoundObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTask(TaskDTO taskDTO,
                                                [FromServices]IValidator<TaskDTO> updateValidator,
                                                CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await updateValidator.ValidateAsync(taskDTO, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors);
            }

            var updatedTask = await _taskService.UpdateAsync(taskDTO, cancellationToken);
            if (updatedTask.IsFailed)
            {
                return new BadRequestObjectResult(new { Errors = updatedTask.Errors.Select(e => e.Message) });
            }

            return new OkObjectResult(updatedTask.Value);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="nameof(id)"></param>
    /// <returns><see cref="NoContentResult"/></returns>
    /// <remarks>
    /// Usage Example:
    /// DELETE task/id
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="204">No Content</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent, Type = typeof(NoContentResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, Type = typeof(BadHttpRequestException))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, Type = typeof(NotFoundObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTask(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(new { Error = Constants.VALIDATION_TASK_ID_NOT_EMPTY });
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
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Assign a task to a user.
    /// </summary>
    /// <param name="nameof(id)"></param>
    /// <param name="nameof(email)"></param>
    /// <returns><see cref="TaskDTO"/></returns>
    /// <remarks>
    /// Usage Example:
    /// PATCH task/id/assign/email
    ///
    /// Headers
    /// Accept: application/json
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch($"{{id}}/assign/{{email}}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, Type = typeof(NoContentResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, Type = typeof(NotFoundObjectResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignedUserToATask(
        Guid id,
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(new { Error = Constants.VALIDATION_TASK_ID_NOT_EMPTY });
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return new BadRequestObjectResult(new { Error = Constants.VALIDATION_USER_EMAIL_NOT_EMPTY });
            }

            var task = await _taskService.AssignTaskToUserAsync(id, email, cancellationToken);
            if (task == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(task);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
    }
}
