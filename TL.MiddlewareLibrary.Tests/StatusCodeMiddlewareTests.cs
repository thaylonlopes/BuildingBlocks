using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TL.BaseContracts;
using TL.MiddlewareLibrary.Exceptions;
using TL.MiddlewareLibrary.Middlewares;
using TL.MiddlewareLibrary.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace TL.MiddlewareLibrary.Tests
{
    public class StatusCodeMiddlewareTests
    {
        private readonly Mock<ILogger<StatusCodeMiddleware>> _loggerMock = new();

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsNotFoundException_ShouldReturn404ProblemDetails()
        {
            var context = new DefaultHttpContext();
            context.TraceIdentifier = "test-trace-123";
            context.Request.Path = "/api/produtos/99";
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new NotFoundException("Produto não encontrado.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(404, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);

            responseBody.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
            var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(json);

            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Not Found", problemDetails.Title);
            Assert.Equal("Produto não encontrado.", problemDetails.Detail);
            Assert.Equal("/api/produtos/99", problemDetails.Instance);
            Assert.Equal("test-trace-123", problemDetails.TraceId);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsBadRequestException_ShouldReturn400ProblemDetails()
        {
            var context = new DefaultHttpContext();
            context.TraceIdentifier = "bad-req-trace";
            context.Request.Path = "/api/clientes";
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new BadRequestException("Payload inválido.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(400, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);

            responseBody.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
            var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(json);

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.Equal("Bad Request", problemDetails.Title);
            Assert.Equal("Payload inválido.", problemDetails.Detail);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsValidationExceptionWithErrors_ShouldReturn400AndErrorsDictionary()
        {
            var context = new DefaultHttpContext();
            context.TraceIdentifier = "validation-trace-456";
            context.Request.Path = "/api/usuarios";
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var failures = new Dictionary<string, string[]>
            {
                { "Email", new[] { "E-mail é obrigatório.", "Formato de e-mail inválido." } },
                { "Senha", new[] { "Senha deve ter no mínimo 8 caracteres." } }
            };
            var validationError = ValidationError.FromFailures(failures, "Dados de usuário inválidos.", "User.InvalidInput");

            RequestDelegate next = _ => throw new ValidationException(validationError);
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(400, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);

            responseBody.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
            var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(json);

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.Equal("Bad Request", problemDetails.Title);
            Assert.Equal("User.InvalidInput", problemDetails.Code);
            Assert.NotNull(problemDetails.Errors);
            Assert.True(problemDetails.Errors.ContainsKey("Email"));
            Assert.Equal(2, problemDetails.Errors["Email"].Length);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsUnauthorizedAccessException_ShouldReturn401ProblemDetails()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new UnauthorizedAccessException("Credenciais inválidas.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(401, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsForbiddenException_ShouldReturn403ProblemDetails()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new ForbiddenException("Acesso restrito a administradores.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(403, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsConflictException_ShouldReturn409ProblemDetails()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new ConflictException("Já existe um registro com o mesmo CPF.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(409, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsTooManyRequestsException_ShouldReturn429ProblemDetails()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new TooManyRequestsException("Cota atingida.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(429, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsBadGatewayException_ShouldReturn502ProblemDetails()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new BadGatewayException("Serviço parceiro indisponível.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(502, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsGatewayTimeoutException_ShouldReturn504ProblemDetails()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new GatewayTimeoutException("Tempo limite esgotado no gateway.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(504, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_WhenDownstreamThrowsGenericException_ShouldReturn500ProblemDetails()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            RequestDelegate next = _ => throw new InvalidOperationException("Falha inesperada no banco.");
            var middleware = new StatusCodeMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }
    }
}
