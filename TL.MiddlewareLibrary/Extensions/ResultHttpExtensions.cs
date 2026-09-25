using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using TL.BaseContracts;

namespace TL.BaseContracts.Http;

/// <summary>
/// Extensões fluentes para conversão de instâncias de Result e Result de T em IResult no padrão RFC 7807 (ProblemDetails).
/// </summary>
public static class ResultHttpExtensions
{
    /// <summary>
    /// Converte um Result genérico em IResult HTTP (200 OK ou ProblemDetails).
    /// </summary>
    /// <typeparam name="T">Tipo do valor transportado no resultado.</typeparam>
    /// <param name="result">Instância do resultado da operação.</param>
    /// <param name="onSuccess">Delegate opcional para customização da resposta de sucesso (ex.: 201 Created).</param>
    /// <returns>Instância de <see cref="IResult"/> correspondente ao estado do resultado.</returns>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return onSuccess is not null
                ? onSuccess(result.Value)
                : Results.Ok(result.Value);
        }

        return MapErrorToHttpResult(result.Error);
    }

    /// <summary>
    /// Converte um Result void em IResult HTTP (200 OK ou ProblemDetails).
    /// </summary>
    /// <param name="result">Instância do resultado da operação.</param>
    /// <param name="onSuccess">Delegate opcional para customização da resposta de sucesso (ex.: 204 NoContent).</param>
    /// <returns>Instância de <see cref="IResult"/> correspondente ao estado do resultado.</returns>
    public static IResult ToHttpResult(this Result result, Func<IResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return onSuccess is not null
                ? onSuccess()
                : Results.Ok();
        }

        return MapErrorToHttpResult(result.Error);
    }

    private static IResult MapErrorToHttpResult(Error error)
    {
        if (error is ValidationError validationError)
        {
            return CreateValidationProblemResult(validationError);
        }

        return CreateStandardProblemResult(error);
    }

    private static IResult CreateValidationProblemResult(ValidationError validationError)
    {
        var errorsDictionary = validationError.Errors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value);

        return Results.ValidationProblem(
            errors: errorsDictionary,
            title: validationError.Code,
            detail: validationError.Message,
            statusCode: StatusCodes.Status400BadRequest,
            extensions: CreateErrorCodeExtension(validationError.Code));
    }

    private static IResult CreateStandardProblemResult(Error error)
    {
        var statusCode = ResolveStatusCodeFromErrorType(error.Type);

        return Results.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Message,
            extensions: CreateErrorCodeExtension(error.Code));
    }

    private static int ResolveStatusCodeFromErrorType(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static Dictionary<string, object?> CreateErrorCodeExtension(string errorCode)
    {
        return new Dictionary<string, object?>
        {
            ["errorCode"] = errorCode
        };
    }
}
