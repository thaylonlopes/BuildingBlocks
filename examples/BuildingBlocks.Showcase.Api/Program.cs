using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL.HealthCheck;
using TL.HealthCheck.Config;
using TL.BaseContracts;
using TL.BaseContracts.Messaging;
using BuildingBlocks.Showcase.Api.Events;
using BuildingBlocks.Showcase.Api.Models;
using BuildingBlocks.Showcase.Api.Services;
using TL.MiddlewareLibrary.Exceptions;
using TL.MiddlewareLibrary.Extensions;
using TL.MiddlewareLibrary.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BuildingBlocks Showcase API",
        Version = "v1",
        Description = "API executável de demonstração e guia de uso dos pacotes da suíte TL.BuildingBlocks (.NET 8 / .NET 9)."
    });
});

builder.Services.AddRequiredHealthChecks(() => new[]
{
    new CheckConfig("database_sql", new[] { "ready" }, () => HealthCheckResult.Healthy("Conexão SQL saudável."), TimeSpan.FromSeconds(2)),
    new CheckConfig("event_bus", new[] { "ready" }, () => HealthCheckResult.Healthy("Event Bus in-memory ativo."), TimeSpan.FromSeconds(1)),
    new CheckConfig("api_liveness", new[] { "live" }, () => HealthCheckResult.Healthy("Processo em execução saudável."), TimeSpan.FromSeconds(1))
});

builder.Services.AddSingleton<IEventProducer, InMemoryEventProducer>();
builder.Services.AddSingleton<OrderService>();
builder.Services.AddTransient<OrderCreatedHandler>();

var app = builder.Build();

app.UseRequestTiming();
app.UseRateLimiting(limit: 50, period: TimeSpan.FromMinutes(1));
app.UseCachingMiddleware(cacheDuration: TimeSpan.FromSeconds(30));
app.UseExceptionHandling();
app.UseStatusCodeMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BuildingBlocks Showcase v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapRequiredHealthCheck();

var ordersGroup = app.MapGroup("/api/orders").WithTags("Pedidos (Orders)");

ordersGroup.MapGet("/", (
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 5,
    [FromServices] OrderService orderService = null!) =>
{
    var pagedRequest = new PagedRequest(pageNumber, pageSize);
    var pagedResult = orderService.GetOrdersPaged(pagedRequest);
    return Results.Ok(pagedResult);
})
.WithName("GetOrdersPaged")
.WithSummary("Lista pedidos com paginação automática usando PagedRequest e PagedResult<T>.");

ordersGroup.MapGet("/{id:guid}", (
    [FromRoute] Guid id,
    [FromServices] OrderService orderService = null!) =>
{
    var result = orderService.GetOrderById(id);

    return result.Match(
        order => Results.Ok(order),
        error => error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
            _ => Results.BadRequest(new { error.Code, error.Message })
        });
})
.WithName("GetOrderById")
.WithSummary("Busca um pedido por ID utilizando Result<OrderDto> e Pattern Matching Match().");

ordersGroup.MapPost("/", async (
    [FromBody] CreateOrderRequest request,
    [FromServices] OrderService orderService = null!) =>
{
    var result = await orderService.CreateOrderAsync(request);

    return result.Match(
        order => Results.Created($"/api/orders/{order.Id}", order),
        error =>
        {
            if (error is ValidationError validationError)
            {
                var errorsDict = validationError.Errors.ToDictionary(k => k.Key, v => v.Value);
                return Results.ValidationProblem(
                    errors: errorsDict,
                    title: validationError.Message,
                    detail: validationError.Code);
            }

            return Results.BadRequest(new { error.Code, error.Message });
        });
})
.WithName("CreateOrder")
.WithSummary("Cria um novo pedido com validação rica por campo (ValidationError) e disparo assíncrono via IEventProducer.");

app.MapGet("/api/diagnostics/reflection-demo", () =>
{
    var sampleCalculator = new SampleCalculator();
    
    #pragma warning disable CS0618 
    int calculated = TL.InvokePrivate.MethodInvoker.InvokePrivateMethod<int>(
        sampleCalculator, "ComputeSecretMultiplier", 7);
    #pragma warning restore CS0618

    return Results.Ok(new
    {
        Message = "Demonstração de invocação de método privado via MethodInvoker com desembrulho de exceções.",
        Input = 7,
        Result = calculated
    });
})
.WithTags("Diagnósticos")
.WithSummary("Demonstra o uso controlado de Reflection do pacote TL.InvokePrivate.");

var middlewareGroup = app.MapGroup("/api/middlewares").WithTags("Middlewares & RFC 7807 (TL.MiddlewareLibrary)");

middlewareGroup.MapGet("/timing", () => Results.Ok(new
{
    Message = "Esta requisição teve sua latência mensurada pelo RequestTimingMiddleware. Inspecione o cabeçalho HTTP X-Response-Time-Ms.",
    TimestampUtc = DateTimeOffset.UtcNow
}))
.WithSummary("Demonstra a medição de tempo de resposta no cabeçalho X-Response-Time-Ms.");

middlewareGroup.MapGet("/cache", ([FromQuery] string? category) => Results.Ok(new
{
    Message = "Resposta armazenada em cache pelo CachingMiddleware por 30 segundos. Verifique o cabeçalho X-Cache (HIT/MISS).",
    Category = category ?? "todos",
    TimestampUtc = DateTimeOffset.UtcNow
}))
.WithSummary("Demonstra o cache em memória transparente para requisições GET idempotentes.");

middlewareGroup.MapGet("/validation-error", () =>
{
    var failures = new Dictionary<string, string[]>
    {
        { "Documento", new[] { "CPF informado é inválido.", "Formato deve conter 11 dígitos numéricos." } },
        { "LimiteCredito", new[] { "Limite de crédito excede o teto autorizado de R$ 50.000,00." } }
    };
    var validationError = ValidationError.FromFailures(failures, "Falha de validação dos dados cadastrais.", "Customer.InvalidData");
    throw new ValidationException(validationError);
})
.WithSummary("Demonstra resposta de erro RFC 7807 (400 Bad Request) com detalhamento por campo do ValidationError.");

middlewareGroup.MapGet("/not-found-error", () =>
{
    throw new NotFoundException("O recurso solicitado com identificador informado não foi localizado.");
})
.WithSummary("Demonstra resposta de erro RFC 7807 (404 Not Found) capturada pelo StatusCodeMiddleware.");

middlewareGroup.MapGet("/unhandled-crash", () =>
{
    throw new InvalidOperationException("Simulação de falha catastrófica não tratada capturada pelo ExceptionHandlingMiddleware.");
})
.WithSummary("Demonstra fallback global RFC 7807 (500 Internal Server Error) com Correlation ID.");

app.Run();

public sealed class InMemoryEventProducer : IEventProducer
{
    private readonly ILogger<InMemoryEventProducer> _logger;

    public InMemoryEventProducer(ILogger<InMemoryEventProducer> logger)
    {
        _logger = logger;
    }

    public Task<Result> PublishAsync<T>(T message, EventMetadata? metadata = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("Evento {EventType} publicado em memória", typeof(T).Name);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> PublishBatchAsync<T>(IEnumerable<T> messages, EventMetadata? metadata = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("Lote de eventos {EventType} publicado em memória", typeof(T).Name);
        return Task.FromResult(Result.Success());
    }
}

public class SampleCalculator
{
    private int ComputeSecretMultiplier(int value) => value * 42;
}

public partial class Program { }