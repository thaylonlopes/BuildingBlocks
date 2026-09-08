using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonHelpers.HealthCheck;
using CommonHelpers.HealthCheck.Config;
using TL.BaseContracts;
using TL.BaseContracts.Messaging;
using CommonHelpers.Showcase.Api.Events;
using CommonHelpers.Showcase.Api.Models;
using CommonHelpers.Showcase.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CommonHelpers Showcase API",
        Version = "v1",
        Description = "API executável de demonstração e guia de uso dos pacotes da suíte CommonHelpers (.NET 8 / .NET 9)."
    });
});

builder.Services.AddRequiredHealthChecks(() => new[]
{
    new CheckConfig("database_sql", new[] { "ready" }, () => HealthCheckResult.Healthy("ConexÃ£o SQL saudÃ¡vel."), TimeSpan.FromSeconds(2)),
    new CheckConfig("event_bus", new[] { "ready" }, () => HealthCheckResult.Healthy("Event Bus in-memory ativo."), TimeSpan.FromSeconds(1)),
    new CheckConfig("api_liveness", new[] { "live" }, () => HealthCheckResult.Healthy("Processo em execuÃ§Ã£o saudÃ¡vel."), TimeSpan.FromSeconds(1))
});

builder.Services.AddSingleton<IEventProducer, InMemoryEventProducer>();
builder.Services.AddSingleton<OrderService>();
builder.Services.AddTransient<OrderCreatedHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CommonHelpers Showcase v1");
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
.WithSummary("Lista pedidos com paginaÃ§Ã£o automÃ¡tica usando PagedRequest e PagedResult<T>.");

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
.WithSummary("Cria um novo pedido com validaÃ§Ã£o rica por campo (ValidationError) e disparo assÃ­ncrono via IEventProducer.");

app.MapGet("/api/diagnostics/reflection-demo", () =>
{
    var sampleCalculator = new SampleCalculator();
    
    #pragma warning disable CS0618 
    int calculated = CommonHelpers.InvokePrivate.MethodInvoker.InvokePrivateMethod<int>(
        sampleCalculator, "ComputeSecretMultiplier", 7);
    #pragma warning restore CS0618

    return Results.Ok(new
    {
        Message = "DemonstraÃ§Ã£o de invocaÃ§Ã£o de mÃ©todo privado via MethodInvoker com desembrulho de exceÃ§Ãµes.",
        Input = 7,
        Result = calculated
    });
})
.WithTags("DiagnÃ³sticos")
.WithSummary("Demonstra o uso controlado de Reflection do pacote CommonHelpers.InvokePrivate.");

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
        _logger.LogInformation("Evento {EventType} publicado em memÃ³ria", typeof(T).Name);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> PublishBatchAsync<T>(IEnumerable<T> messages, EventMetadata? metadata = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogInformation("Lote de eventos {EventType} publicado em memÃ³ria", typeof(T).Name);
        return Task.FromResult(Result.Success());
    }
}

public class SampleCalculator
{
    private int ComputeSecretMultiplier(int value) => value * 42;
}

public partial class Program { }