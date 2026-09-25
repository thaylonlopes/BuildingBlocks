# 🚀 TL.MiddlewareLibrary

[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> **Suíte modular de middlewares para ASP.NET Core (.NET 8 e .NET 9): diagnóstico de latência com zero alocação, controle de taxa por IP, cache em memória com pooling e respostas estruturadas de erro no padrão RFC 7807 (ProblemDetails) integradas a TL.BaseContracts.**

O **`TL.MiddlewareLibrary`** é uma biblioteca de infraestrutura transversal para o pipeline HTTP do ASP.NET Core, concebida para simplificar o desenvolvimento de APIs modernas, eliminar código defensivo repetitivo em controllers/endpoints e padronizar o tratamento de exceções de negócio e falhas não tratadas.

---

## 📦 Instalação

Adicione o pacote via .NET CLI:

```bash
dotnet add package TL.MiddlewareLibrary
```

---

## 🚀 Métodos de Extensão do Pipeline

| Extensão | Middleware | Descrição Resumida |
| :--- | :--- | :--- |
| `app.UseRequestTiming()` | `RequestTimingMiddleware` | Mede a latência da requisição com zero alocação e injeta o header `X-Response-Time-Ms`. |
| `app.UseRateLimiting(limit, period)` | `RateLimitingMiddleware` | Protege a API contra rajadas excessivas de chamadas por IP, retornando HTTP 429. |
| `app.UseAuthenticationMiddleware()` | `AuthenticationMiddleware` | Validação ágil de cabeçalho `Authorization` no início do fluxo. |
| `app.UseCachingMiddleware(duration)` | `CachingMiddleware` | Cache em memória transparente para requisições `GET` e `HEAD` (200 OK) com chave composta e pooling de buffers. |
| `app.UseExceptionHandling()` / `app.UseGlobalExceptionHandling()` | `ExceptionHandlingMiddleware` | Captura global de exceções imprevistas, gerando resposta RFC 7807 amigável com `traceId` e `X-Correlation-Id`. |
| `app.UseStatusCodeMiddleware()` | `StatusCodeMiddleware` | Mapeia exceções de domínio tipadas e contratos de erro (`Error`, `ValidationError` de `TL.BaseContracts`). |

---

## 💡 Exemplo Rápido de Uso

```csharp
using TL.MiddlewareLibrary.Exceptions;
using TL.MiddlewareLibrary.Extensions;
using TL.BaseContracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseRequestTiming();
app.UseRateLimiting(100, TimeSpan.FromMinutes(1));
app.UseCachingMiddleware(TimeSpan.FromMinutes(2));
app.UseExceptionHandling();
app.UseStatusCodeMiddleware();

app.MapGet("/api/clientes/{id}", (int id) =>
{
    if (id <= 0)
        throw new BadRequestException("O ID informado é inválido.");

    return Results.Ok(new { id, nome = "Cliente Exemplo" });
});

app.Run();
```

---

## 🎯 Ergonomia Minimal APIs (`ToHttpResult()`)

Elimine o código defensivo repetitivo de pattern matching nas Minimal APIs com a bridge `ToHttpResult()`:

```csharp
using TL.BaseContracts.Http;

app.MapGet("/api/pedidos/{id:guid}", (Guid id, PedidoService service) =>
{
    return service.ObterPorId(id).ToHttpResult();
});

app.MapPost("/api/pedidos", async (NovoPedidoRequest request, PedidoService service) =>
{
    var result = await service.CriarPedidoAsync(request);
    return result.ToHttpResult(pedido => Results.Created($"/api/pedidos/{pedido.Id}", pedido));
});
```

Erros são mapeados automaticamente para RFC 7807 (`ProblemDetails` e `ValidationProblemDetails`).

---

## 👤 Injeção de Identidade e Tenant (`ICurrentUser` e `ICurrentTenant`)

Registre as implementações integradas ao `IHttpContextAccessor`:

```csharp
using TL.MiddlewareLibrary.Context;

builder.Services.AddCurrentUser();
builder.Services.AddCurrentTenant(headerName: "X-Tenant-Id");
```

Permite injetar `ICurrentUser` e `ICurrentTenant` diretamente nos serviços de aplicação.

---

## 📄 Licença

Distribuído sob a licença [MIT](https://opensource.org/licenses/MIT).

