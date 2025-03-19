using System.Collections.Generic;
using System;

namespace Presentation.DTOs;

public class ApiErrorResponse
{
    public List<string> Errors { get; set; } = [];

    public static ApiErrorResponse Build(string error)
    {
        return new ApiErrorResponse { Errors = [error] };
    }

    public static ApiErrorResponse Build(IEnumerable<string> errors)
    {
        return new ApiErrorResponse { Errors = errors?.ToList() ?? throw new ArgumentNullException(nameof(errors)) };
    }

    public static ApiErrorResponse Build(List<FluentResults.IError> errors)
    {
        return new ApiErrorResponse { Errors = errors?.Select(e => e.Message).ToList() ?? throw new ArgumentNullException(nameof(errors)) };
    }

    public static ApiErrorResponse Build(List<FluentValidation.Results.ValidationFailure> errors)
    {
        return new ApiErrorResponse { Errors = errors?.Select(e => e.ErrorMessage).ToList() ?? throw new ArgumentNullException(nameof(errors)) };
    }
}