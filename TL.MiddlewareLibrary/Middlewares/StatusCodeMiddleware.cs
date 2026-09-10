using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TL.BaseContracts;
using TL.MiddlewareLibrary.Exceptions;
using TL.MiddlewareLibrary.Models;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TL.MiddlewareLibrary.Middlewares
{
    /// <summary>
    /// Middleware responsável por interceptar exceções tipadas de negócio e traduzi-las em respostas estruturadas (ProblemDetails RFC 7807) integradas a TL.BaseContracts.
    /// </summary>
    public class StatusCodeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StatusCodeMiddleware> _logger;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="StatusCodeMiddleware"/>.
        /// </summary>
        public StatusCodeMiddleware(RequestDelegate next, ILogger<StatusCodeMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executa a interceptação assíncrona do pipeline HTTP.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("A resposta HTTP já foi iniciada; não é possível reescrever os cabeçalhos para o ProblemDetails.");
                return;
            }

            var traceId = ResolveTraceId(context);
            var (statusCode, problemDetails) = MapExceptionToProblemDetails(exception, context.Request.Path.Value, traceId);

            _logger.LogError(exception, "Exceção capturada pelo StatusCodeMiddleware ({StatusCode}): {Message}", (int)statusCode, exception.Message);

            context.Response.Clear();
            EnsureCorrelationHeader(context, traceId);
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }

        private static (HttpStatusCode StatusCode, ProblemDetailsResponse Response) MapExceptionToProblemDetails(
            Exception exception,
            string? instance,
            string traceId)
        {
            if (exception is ValidationException validationException && validationException.ValidationError != null)
            {
                return (HttpStatusCode.BadRequest, ProblemDetailsResponse.FromValidationError(validationException.ValidationError, instance, traceId));
            }

            if (exception is BadRequestException badRequestException && badRequestException.Error != null)
            {
                return (HttpStatusCode.BadRequest, ProblemDetailsResponse.FromError(badRequestException.Error, instance, traceId, 400));
            }

            var (statusCode, title) = ResolveStatusAndTitle(exception);

            var detail = statusCode == HttpStatusCode.InternalServerError
                ? "Ocorreu um erro interno ao processar a solicitação."
                : exception.Message;

            var response = ProblemDetailsResponse.Create(
                (int)statusCode,
                title,
                detail,
                instance,
                traceId,
                ResolveErrorCode(exception));

            return (statusCode, response);
        }

        private static (HttpStatusCode StatusCode, string Title) ResolveStatusAndTitle(Exception exception) => exception switch
        {
            BadRequestException _ or ArgumentNullException _ or ArgumentException _ => (HttpStatusCode.BadRequest, "Bad Request"),
            UnauthorizedAccessException _ or UnauthorizedException _ => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ForbiddenException _ => (HttpStatusCode.Forbidden, "Forbidden"),
            TimeoutException _ => (HttpStatusCode.RequestTimeout, "Request Timeout"),
            NotFoundException _ => (HttpStatusCode.NotFound, "Not Found"),
            NotAcceptableException _ => (HttpStatusCode.NotAcceptable, "Not Acceptable"),
            ConflictException _ => (HttpStatusCode.Conflict, "Conflict"),
            UnsupportedMediaTypeException _ => (HttpStatusCode.UnsupportedMediaType, "Unsupported Media Type"),
            LockedException _ => (HttpStatusCode.Locked, "Locked"),
            TooManyRequestsException _ => (HttpStatusCode.TooManyRequests, "Too Many Requests"),
            BadGatewayException _ => (HttpStatusCode.BadGateway, "Bad Gateway"),
            GatewayTimeoutException _ => (HttpStatusCode.GatewayTimeout, "Gateway Timeout"),
            InsufficientStorageException _ => (HttpStatusCode.InsufficientStorage, "Insufficient Storage"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        private static string? ResolveErrorCode(Exception exception) => exception switch
        {
            ArgumentNullException _ or ArgumentException _ or BadRequestException _ => "Http.BadRequest",
            UnauthorizedAccessException _ or UnauthorizedException _ => "Http.Unauthorized",
            ForbiddenException _ => "Http.Forbidden",
            TimeoutException _ => "Http.RequestTimeout",
            NotFoundException _ => "Http.NotFound",
            NotAcceptableException _ => "Http.NotAcceptable",
            ConflictException _ => "Http.Conflict",
            UnsupportedMediaTypeException _ => "Http.UnsupportedMediaType",
            LockedException _ => "Http.Locked",
            TooManyRequestsException _ => "Http.TooManyRequests",
            BadGatewayException _ => "Http.BadGateway",
            GatewayTimeoutException _ => "Http.GatewayTimeout",
            InsufficientStorageException _ => "Http.InsufficientStorage",
            _ => "Server.InternalError"
        };

        private static string ResolveTraceId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationHeader) && !string.IsNullOrWhiteSpace(correlationHeader))
            {
                return correlationHeader.ToString();
            }

            return context.TraceIdentifier ?? Guid.NewGuid().ToString();
        }

        private static void EnsureCorrelationHeader(HttpContext context, string traceId)
        {
            if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
            {
                context.Response.Headers.Append("X-Correlation-Id", traceId);
            }
        }
    }
}
