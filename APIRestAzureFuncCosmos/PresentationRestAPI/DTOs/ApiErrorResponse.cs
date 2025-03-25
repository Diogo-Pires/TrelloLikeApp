namespace PresentationRestAPI.DTOs;

public class ApiErrorResponse
{
    public List<string> Errors { get; set; } = [];

    public static ApiErrorResponse Build(string error) =>
        new() { Errors = [error] };

    public static ApiErrorResponse Build(IEnumerable<string> errors) =>
        new() { Errors = errors?.ToList() ?? throw new ArgumentNullException(nameof(errors)) };

    public static ApiErrorResponse Build(List<FluentResults.IError> errors) =>
        new() { Errors = errors?.Select(e => e.Message).ToList() ?? throw new ArgumentNullException(nameof(errors)) };

    public static ApiErrorResponse Build(List<FluentValidation.Results.ValidationFailure> errors) =>
        new() { Errors = errors?.Select(e => e.ErrorMessage).ToList() ?? throw new ArgumentNullException(nameof(errors)) };
}