using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TL.MiddlewareLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TL.MiddlewareLibrary.Middlewares
{
    /// <summary>
    /// Middleware responsável por limitar a taxa de requisições por endereço IP em janelas fixas de tempo.
    /// </summary>
    public class RateLimitingMiddleware : IDisposable
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly Dictionary<string, int> _requestCounts = new();
        private readonly int _limit;
        private readonly TimeSpan _period;
        private readonly Timer? _timer;
        private bool _disposed;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="RateLimitingMiddleware"/>.
        /// </summary>
        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            int limit,
            TimeSpan period)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "O limite de requisições deve ser maior que zero.");

            if (period <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(period), "O período deve ser maior que zero.");

            _limit = limit;
            _period = period;

            _timer = new Timer(ResetRequestCounts, null, _period, _period);
        }

        private void ResetRequestCounts(object? state)
        {
            lock (_requestCounts)
            {
                _requestCounts.Clear();
            }
        }

        /// <summary>
        /// Executa o controle e validação de taxa de requisições do cliente.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var clientIp = ResolveClientIp(context);

            lock (_requestCounts)
            {
                if (!_requestCounts.TryGetValue(clientIp, out var currentCount))
                {
                    _requestCounts[clientIp] = 1;
                }
                else
                {
                    if (currentCount >= _limit)
                    {
                        _logger.LogWarning("Taxa limite excedida para o IP {ClientIp}. Limite: {Limit} por {Period}", clientIp, _limit, _period);
                        WriteTooManyRequestsResponse(context);
                        return;
                    }

                    _requestCounts[clientIp] = currentCount + 1;
                }
            }

            await _next(context);
        }

        private static string ResolveClientIp(HttpContext context)
        {
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedHeader))
            {
                var ips = forwardedHeader.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";
        }

        private void WriteTooManyRequestsResponse(HttpContext context)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("A resposta HTTP já foi iniciada; não é possível reescrever os cabeçalhos para erro 429.");
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/problem+json";

            var traceId = context.TraceIdentifier ?? Guid.NewGuid().ToString();
            var problemDetails = ProblemDetailsResponse.Create(
                (int)HttpStatusCode.TooManyRequests,
                "Too Many Requests",
                $"Limite de {_limit} requisições por {_period.TotalSeconds} segundos excedido. Tente novamente mais tarde.",
                context.Request.Path.Value,
                traceId,
                "RateLimit.Exceeded"
            );

            var json = JsonSerializer.Serialize(problemDetails);
            context.Response.WriteAsync(json).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Libera os recursos alocados pelo timer de limpeza.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Executa o descarte seguro dos timers.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _timer?.Dispose();
            }

            _disposed = true;
        }
    }
}
