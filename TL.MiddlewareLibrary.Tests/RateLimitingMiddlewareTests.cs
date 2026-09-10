using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TL.MiddlewareLibrary.Middlewares;
using TL.MiddlewareLibrary.Models;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace TL.MiddlewareLibrary.Tests
{
    public class RateLimitingMiddlewareTests
    {
        private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock = new();

        [Fact]
        public async Task InvokeAsync_WhenRemoteIpAddressIsNull_ShouldUseFallbackAndNotThrowNullReferenceException()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = null;
            context.Response.Body = new MemoryStream();

            var executed = false;
            RequestDelegate next = _ =>
            {
                executed = true;
                return Task.CompletedTask;
            };

            using var middleware = new RateLimitingMiddleware(next, _loggerMock.Object, limit: 5, period: TimeSpan.FromSeconds(1));

            var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

            Assert.Null(exception);
            Assert.True(executed);
        }

        [Fact]
        public async Task InvokeAsync_WhenLimitIsExceeded_ShouldBlockAndReturn429ProblemDetails()
        {
            var limit = 2;
            var executedCount = 0;

            RequestDelegate next = _ =>
            {
                executedCount++;
                return Task.CompletedTask;
            };

            using var middleware = new RateLimitingMiddleware(next, _loggerMock.Object, limit: limit, period: TimeSpan.FromMinutes(1));

            var ip = IPAddress.Parse("192.168.1.100");

            for (int i = 0; i < limit; i++)
            {
                var context = CreateContextWithIp(ip);
                await middleware.InvokeAsync(context);
                Assert.Equal(200, context.Response.StatusCode);
            }

            Assert.Equal(limit, executedCount);

            var blockedContext = CreateContextWithIp(ip);
            await middleware.InvokeAsync(blockedContext);

            Assert.Equal(limit, executedCount);
            Assert.Equal(429, blockedContext.Response.StatusCode);
            Assert.Equal("application/problem+json", blockedContext.Response.ContentType);

            blockedContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(blockedContext.Response.Body, Encoding.UTF8).ReadToEndAsync();
            var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(json);

            Assert.NotNull(problemDetails);
            Assert.Equal(429, problemDetails.Status);
            Assert.Equal("Too Many Requests", problemDetails.Title);
            Assert.Contains("excedido", problemDetails.Detail);
        }

        [Fact]
        public async Task InvokeAsync_WhenMultipleDistinctIpsUsed_ShouldTrackCountsSeparately()
        {
            var limit = 1;
            var executedCount = 0;

            RequestDelegate next = _ =>
            {
                executedCount++;
                return Task.CompletedTask;
            };

            using var middleware = new RateLimitingMiddleware(next, _loggerMock.Object, limit: limit, period: TimeSpan.FromMinutes(1));

            var contextIp1 = CreateContextWithIp(IPAddress.Parse("10.0.0.1"));
            var contextIp2 = CreateContextWithIp(IPAddress.Parse("10.0.0.2"));

            await middleware.InvokeAsync(contextIp1);
            await middleware.InvokeAsync(contextIp2);

            Assert.Equal(2, executedCount);
            Assert.Equal(200, contextIp1.Response.StatusCode);
            Assert.Equal(200, contextIp2.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WhenXForwardedForHeaderPresent_ShouldExtractClientIpCorrectly()
        {
            var limit = 1;
            var executedCount = 0;

            RequestDelegate next = _ =>
            {
                executedCount++;
                return Task.CompletedTask;
            };

            using var middleware = new RateLimitingMiddleware(next, _loggerMock.Object, limit: limit, period: TimeSpan.FromMinutes(1));

            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "203.0.113.195, 70.41.3.18, 150.172.238.178";
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);
            Assert.Equal(200, context.Response.StatusCode);

            var contextSecond = new DefaultHttpContext();
            contextSecond.Request.Headers["X-Forwarded-For"] = "203.0.113.195";
            contextSecond.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(contextSecond);
            Assert.Equal(429, contextSecond.Response.StatusCode);
        }

        private static DefaultHttpContext CreateContextWithIp(IPAddress ip)
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = ip;
            context.Response.Body = new MemoryStream();
            return context;
        }
    }
}
