using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TL.MiddlewareLibrary.Middlewares
{
    /// <summary>
    /// Middleware responsável por gerenciar cache de respostas HTTP em memória para requisições idempotentes seguras (GET e HEAD).
    /// </summary>
    public class CachingMiddleware
    {
        private const int DefaultBufferSize = 4096;
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingMiddleware> _logger;
        private readonly TimeSpan _cacheDuration;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="CachingMiddleware"/>.
        /// </summary>
        public CachingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<CachingMiddleware> logger,
            TimeSpan? cacheDuration = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// Executa o fluxo de verificação e armazenamento de cache HTTP com pooling de buffers de memória.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var cacheKey = GenerateCacheKeyFromRequest(context.Request);

            if (_cache.TryGetValue(cacheKey, out var cachedResponse) && cachedResponse is string responseString)
            {
                _logger.LogInformation("Cache hit para a chave: {CacheKey}", cacheKey);
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.Headers.Append("X-Cache", "HIT");
                await context.Response.WriteAsync(responseString);
                return;
            }

            context.Response.Headers.Append("X-Cache", "MISS");

            var originalBodyStream = context.Response.Body;
            await using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await _next(context);

                if (IsCacheableResponse(context.Response.StatusCode))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var responseBody = await ReadStreamWithBufferPoolAsync(memoryStream);
                    _cache.Set(cacheKey, responseBody, _cacheDuration);
                    _logger.LogInformation("Resposta armazenada em cache para a chave: {CacheKey}", cacheKey);

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalBodyStream);
                }
                else
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private static bool IsCacheableResponse(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }

        private static string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            return $"{request.Path}{request.QueryString}";
        }

        private static async Task<string> ReadStreamWithBufferPoolAsync(Stream stream)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(DefaultBufferSize);
            var stringBuilder = new StringBuilder();

            try
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                }
            }
            finally
            {
                pool.Return(buffer);
            }

            return stringBuilder.ToString();
        }
    }
}
