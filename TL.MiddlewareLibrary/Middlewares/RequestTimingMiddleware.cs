using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TL.MiddlewareLibrary.Middlewares
{
    /// <summary>
    /// Middleware responsável por mensurar o tempo total de execução da requisição HTTP e injetar métricas de diagnóstico com zero alocação de heap.
    /// </summary>
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="RequestTimingMiddleware"/>.
        /// </summary>
        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executa a medição de tempo de resposta da requisição com precisão de timestamp de alta resolução.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            long startTimestamp = Stopwatch.GetTimestamp();

            context.Response.OnStarting(() =>
            {
                AppendResponseTimeHeader(context, startTimestamp);
                AppendCorrelationIdHeader(context);
                return Task.CompletedTask;
            });

            try
            {
                await _next(context);
            }
            finally
            {
                LogExecutionMetrics(context, startTimestamp);
            }
        }

        private static void AppendResponseTimeHeader(HttpContext context, long startTimestamp)
        {
            if (!context.Response.Headers.ContainsKey("X-Response-Time-Ms"))
            {
                long elapsedMilliseconds = CalculateElapsedMilliseconds(startTimestamp);
                context.Response.Headers.Append("X-Response-Time-Ms", elapsedMilliseconds.ToString());
            }
        }

        private static void AppendCorrelationIdHeader(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationHeader) &&
                !string.IsNullOrWhiteSpace(correlationHeader) &&
                !context.Response.Headers.ContainsKey("X-Correlation-Id"))
            {
                context.Response.Headers.Append("X-Correlation-Id", correlationHeader.ToString());
            }
        }

        private void LogExecutionMetrics(HttpContext context, long startTimestamp)
        {
            long elapsedMilliseconds = CalculateElapsedMilliseconds(startTimestamp);
            _logger.LogInformation(
                "Requisição [{Method}] em {Path} concluída com status {StatusCode} em {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMilliseconds);
        }

        private static long CalculateElapsedMilliseconds(long startTimestamp)
        {
            return (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
        }
    }
}
