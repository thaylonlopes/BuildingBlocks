using Microsoft.AspNetCore.Builder;
using TL.MiddlewareLibrary.Middlewares;
using System;

namespace TL.MiddlewareLibrary.Extensions
{
    /// <summary>
    /// Métodos de extensão fluentes para registro dos middlewares da suite TL.MiddlewareLibrary no pipeline HTTP do ASP.NET Core.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Registra o middleware de medição de latência de resposta com zero alocação (Stopwatch de alta precisão).
        /// </summary>
        public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.UseMiddleware<RequestTimingMiddleware>();
        }

        /// <summary>
        /// Registra o middleware de captura e tratamento global de exceções imprevistas (Fallback Catch-All) com formato RFC 7807.
        /// </summary>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        /// <summary>
        /// Alias oficial para <see cref="UseExceptionHandling"/> alinhado à nomenclatura GlobalExceptionHandlingMiddleware.
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseExceptionHandling();
        }

        /// <summary>
        /// Registra o middleware de tradução de exceções tipadas de negócio em respostas estruturadas (ProblemDetails).
        /// </summary>
        public static IApplicationBuilder UseStatusCodeMiddleware(this IApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.UseMiddleware<StatusCodeMiddleware>();
        }

        /// <summary>
        /// Registra o middleware de controle de taxa de requisições por endereço IP.
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, int limit, TimeSpan period)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.UseMiddleware<RateLimitingMiddleware>(limit, period);
        }

        /// <summary>
        /// Registra o middleware de inspeção e validação do cabeçalho de autorização HTTP.
        /// </summary>
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }

        /// <summary>
        /// Registra o middleware de cache em memória para respostas de requisições GET e HEAD com pooling de memória.
        /// </summary>
        public static IApplicationBuilder UseCachingMiddleware(this IApplicationBuilder builder, TimeSpan? cacheDuration = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.UseMiddleware<CachingMiddleware>(cacheDuration ?? TimeSpan.FromMinutes(1));
        }
    }
}
