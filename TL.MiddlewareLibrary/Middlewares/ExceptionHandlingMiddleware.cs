using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TL.MiddlewareLibrary.Models;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TL.MiddlewareLibrary.Middlewares
{
    /// <summary>
    /// Middleware global de tratamento de exceções imprevistas (Fallback Catch-All) com respostas estruturadas em ProblemDetails conforme RFC 7807.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="ExceptionHandlingMiddleware"/>.
        /// </summary>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executa o encapsulamento de segurança contra exceções não tratadas.
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
                _logger.LogError(ex, "Exceção não tratada capturada pelo ExceptionHandlingMiddleware: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("A resposta HTTP já foi iniciada; não é possível reescrever os cabeçalhos para o ProblemDetails de erro 500.");
                return;
            }

            var traceId = ResolveTraceId(context);

            context.Response.Clear();
            EnsureCorrelationHeader(context, traceId);
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problemDetails = ProblemDetailsResponse.FromException(
                exception,
                context.Request.Path.Value,
                traceId
            );

            var result = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(result);
        }

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
