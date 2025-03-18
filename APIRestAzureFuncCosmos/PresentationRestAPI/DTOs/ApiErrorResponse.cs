using System.Collections;

namespace PresentationRestAPI.DTOs;

public class ApiErrorResponse : IEnumerable<string>
{
    private readonly List<string> _errors;

    public ApiErrorResponse(IEnumerable<string> errors)
    {
        _errors = new List<string>(errors ?? throw new ArgumentNullException(nameof(errors)));
    }

    public ApiErrorResponse(List<FluentResults.IError> errors)
    {
        _errors = new List<string>(errors.Select(e => e.Message) ?? throw new ArgumentNullException(nameof(errors)));
    }

    public ApiErrorResponse(List<FluentValidation.Results.ValidationFailure> errors)
    {
        _errors = new List<string>(errors.Select(e => e.ErrorMessage) ?? throw new ArgumentNullException(nameof(errors)));
    }

    public ApiErrorResponse(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error cannot be null or empty.", nameof(error));

        _errors = [error];
    }

    public IReadOnlyList<string> Errors => _errors.AsReadOnly();
    public IEnumerator<string> GetEnumerator() => _errors.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}