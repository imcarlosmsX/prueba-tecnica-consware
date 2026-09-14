using System.Diagnostics;
using ExpenseReimbursement.Application.Common;
using ExpenseReimbursement.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseReimbursement.Api.Middleware;

// DECISIÓN DE DISEÑO: un único middleware traduce excepciones a códigos HTTP. Ningún controller
// tiene try/catch.
// POR QUÉ: si cada acción capturara sus errores, el mapeo excepción -> código estaría repetido y
// un endpoint nuevo podría olvidar un caso y devolver 500 donde tocaba 409.
// CONSECUENCIA: el contrato de errores es uniforme y exhaustivo, y se audita en un solo archivo.
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var problemDetails = BuildProblemDetails(context, exception, traceId);

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogInformation(
                "Request rejected with {StatusCode}: {Message}", problemDetails.Status, exception.Message);
        }

        context.Response.Clear();
        context.Response.StatusCode = problemDetails.Status!.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails BuildProblemDetails(HttpContext context, Exception exception, string traceId)
    {
        var problemDetails = exception switch
        {
            // El estado actual del recurso impide la operación: la petición es válida, el
            // conflicto es de estado. Por eso 409 y no 400.
            InvalidStatusTransitionException conflict => Create(
                StatusCodes.Status409Conflict, "Invalid status transition", conflict.Message),

            DomainValidationException validation => Create(
                StatusCodes.Status400BadRequest, "Validation failed", validation.Message,
                validation.PropertyName),

            // Un id inválido dentro del cuerpo es un error de campo, no una ruta inexistente.
            ReferenceNotFoundException reference => Create(
                StatusCodes.Status400BadRequest, "Validation failed", reference.Message,
                reference.PropertyName),

            NotFoundException notFound => Create(
                StatusCodes.Status404NotFound, "Resource not found", notFound.Message),

            // Nunca se filtra el mensaje interno ni el stack trace: el detalle va al log y el
            // cliente recibe el traceId para poder correlacionarlo con soporte.
            _ => Create(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "The request could not be processed. Contact support with the trace id.")
        };

        problemDetails.Type = $"https://httpstatuses.io/{problemDetails.Status}";
        problemDetails.Instance = $"{context.Request.Method} {context.Request.Path}";
        problemDetails.Extensions["traceId"] = traceId;

        return problemDetails;
    }

    private static ProblemDetails Create(int status, string title, string detail, string? propertyName = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        if (propertyName is not null)
        {
            problemDetails.Extensions["errors"] = new Dictionary<string, string[]>
            {
                [ToCamelCase(propertyName)] = [detail]
            };
        }

        return problemDetails;
    }

    // El JSON de la API es camelCase; el nombre de la propiedad viene del dominio en PascalCase.
    private static string ToCamelCase(string propertyName) =>
        char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
