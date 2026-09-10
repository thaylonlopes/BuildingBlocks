# 💎 TL.BuildingBlocks

> Fundação e espinha dorsal modular em .NET para acelerar a construção de microsserviços, Web APIs e Worker Services com Result Pattern, Health Checks padronizados e contratos de arquitetura.

[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-purple.svg)](https://dotnet.microsoft.com/)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20(LTS)-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/Tests-102%2F102%20Passing-brightgreen.svg)]()

---

## 📑 Sumário

- [🌟 O que é o TL.BuildingBlocks?](#-o-que-é-o-tlbuildingblocks)
- [📦 Pacotes NuGet](#-pacotes-nuget)
- [🚀 Guia de Início Rápido](#-guia-de-início-rápido)
  - [1. Mensageria Agnóstica (Ports & Adapters)](#1-mensageria-agnóstica-ports--adapters)
  - [2. Paginação Padronizada e Result Pattern](#2-paginação-padronizada-e-result-pattern)
  - [3. Health Checks Padronizados em 1 Linha](#3-health-checks-padronizados-em-1-linha)
  - [4. Middlewares HTTP & Erros RFC 7807](#4-middlewares-http--erros-rfc-7807)
  - [5. Showcase & API Executável](#5-showcase--api-executável)
- [🧪 Executando os Testes](#-executando-os-testes)
- [🏛️ Catálogo de Decisões Arquiteturais (ADRs)](#️-catálogo-de-decisões-arquiteturais-adrs)

---

## 🌟 O que é o TL.BuildingBlocks?

O **TL.BuildingBlocks** é a fundação corporativa de blocos fundamentais de infraestrutura, comunicação e resiliência projetada para eliminar código boilerplate repetitivo em microsserviços .NET:

1. **💎 Result Pattern, Paginação & Contratos ([`TL.BaseContracts`](TL.BaseContracts/README.md))**: `Result<T>` funcional com `ErrorType`, `PagedResult<T>` imutável com cálculo automático de páginas, `ValidationError` por campo e portas agnósticas de mensageria (`IEventProducer`, `IEventHandler<T>`, `EventMessage<T>`). **Zero dependências externas**.
2. **🩺 Health Checks Padronizados ([`TL.HealthCheck`](TL.HealthCheck/README.md))**: Probes de *Liveness* (`/liveness`), *Readiness* (`/ready`) e dashboard com UI Client (`/health`) em uma única linha, para Web APIs e Worker Services em segundo plano.
3. **🛡️ Middlewares HTTP & RFC 7807 ([`TL.MiddlewareLibrary`](TL.MiddlewareLibrary/README.md))**: Suíte modular de middlewares para ASP.NET Core com medição de latência zero-allocation (`X-Response-Time-Ms`), controle de taxa por IP, cache em memória seguro e respostas de erro estruturadas em `ProblemDetails` integradas a `TL.BaseContracts`.
4. **🐰 Adaptador RabbitMQ ([`TL.RabbitMQ`](https://github.com/thaylonlopes/TL.Messaging))**: Implementação de `IEventProducer` e `IRabbitMqProducer` com Publisher Confirms, Dead-Letter Queue (`.dlq`) automática e retentativas com Polly (migrado para repositório [`TL.Messaging`](https://github.com/thaylonlopes/TL.Messaging)).
5. **🦅 Adaptador Apache Kafka ([`TL.Kafka`](https://github.com/thaylonlopes/TL.Messaging))**: Implementação de `IEventProducer` e `IKafkaProducer` com Partition Keys, Dead Letter Topic (`.dlt`), idempotência e headers de telemetria (migrado para repositório [`TL.Messaging`](https://github.com/thaylonlopes/TL.Messaging)).
6. **🔍 Invocação Segura de Membros Privados ([`TL.InvokePrivate`](TL.InvokePrivate/README.md))**: Utilitário baseado em Reflection para testes e código legado com desembrulho de `TargetInvocationException` (aposentado, sem empacotamento).

---

## 📦 Pacotes NuGet

| Pacote NuGet | Versão | Runtimes Suportados | Descrição & Documentação |
| :--- | :---: | :--- | :--- |
| [**`TL.BaseContracts`**](TL.BaseContracts/README.md) | `0.2.0` | `netstandard2.0; net8.0; net9.0` | Fundação padronizada de contratos em BCL pura (Commands, Queries, Events, Results, Errors e Paginações). |
| [**`TL.HealthCheck`**](TL.HealthCheck/README.md) | `0.2.0` | `net8.0; net9.0` | Probes de Liveness, Readiness, UI Client e Web Host para Workers. |
| [**`TL.MiddlewareLibrary`**](TL.MiddlewareLibrary/README.md) | `0.2.0` | `net8.0; net9.0` | Suíte modular de middlewares HTTP (latência, rate limiting, cache, RFC 7807 ProblemDetails). |
| [**`TL.RabbitMQ`**](https://github.com/thaylonlopes/TL.Messaging) *(migrado)* | `0.1.0` | `net8.0; net9.0` | Adaptador RabbitMQ com Publisher Confirms e DLQ (migrado para [`TL.Messaging`](https://github.com/thaylonlopes/TL.Messaging)). |
| [**`TL.Kafka`**](https://github.com/thaylonlopes/TL.Messaging) *(migrado)* | `0.1.0` | `net8.0; net9.0` | Adaptador Kafka com Partition Keys, Idempotência e DLT (migrado para [`TL.Messaging`](https://github.com/thaylonlopes/TL.Messaging)). |
| [**`TL.InvokePrivate`**](TL.InvokePrivate/README.md) *(aposentado)* | `0.1.0` | `net6.0; net8.0; net9.0` | Reflection para legados (mantido para compatibilidade, sem empacotamento). |

---

## Guia de Início Rápido

### 1. Mensageria Agnóstica (Ports & Adapters)

Troque de broker de mensageria alterando apenas a injeção de dependência no `Program.cs`, sem tocar na regra de negócio:

```csharp
// Opção A: Usando RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Opção B: Usando Apache Kafka (Sem alterar nenhum handler ou producer!)
// builder.Services.AddKafkaMessaging(builder.Configuration);
```

Na regra de negócio, injete apenas a interface agnóstica `IEventProducer`:

```csharp
using TL.BaseContracts.Messaging;

public class OrderService
{
    private readonly IEventProducer _producer;

    public OrderService(IEventProducer producer) => _producer = producer;

    public async Task CreateOrderAsync(OrderDto order, CancellationToken ct)
    {
        var orderEvent = new OrderCreatedEvent(order.Id, order.Total);
        await _producer.PublishAsync(orderEvent, cancellationToken: ct);
    }
}
```

---

### 2. Paginação Padronizada e Result Pattern

```csharp
using TL.BaseContracts;

[HttpGet("users")]
public async Task<IActionResult> GetUsers([FromQuery] PagedRequest request)
{
    var (users, totalCount) = await _userRepo.GetPagedAsync(request.PageNumber, request.PageSize);
    var pagedResult = PagedResult<UserDto>.Create(users, totalCount, request.PageNumber, request.PageSize);
    return Ok(pagedResult);
}
```

---

### 3. Health Checks Padronizados em 1 Linha

```csharp
using TL.HealthCheck;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRequiredHealthChecks();

var app = builder.Build();
app.MapRequiredHealthCheck(); // Expõe /health, /liveness e /ready

app.Run();
```

---

### 4. Showcase & API Executável

Criamos um projeto completo e executável em [`examples/BuildingBlocks.Showcase.Api`](examples/BuildingBlocks.Showcase.Api) com **Swagger UI** demonstrando na prática o uso integrado dos pacotes.

Para executar localmente:

```powershell
dotnet run --project examples/BuildingBlocks.Showcase.Api
```
Acesse o Swagger interativo em: **`http://localhost:5000`**.

---

## 🧪 Executando os Testes

Para rodar todos os **102 testes automatizados** (unitários e integração) da solução `BuildingBlocks.sln`:

```powershell
dotnet test BuildingBlocks.sln
```

---

## 🏛️ Catálogo de Decisões Arquiteturais (ADRs)

- [**`ADR-000: Arquitetura e Convenções da Suíte TL.BuildingBlocks`**](docs/adr/ADR-000-arquitetura-e-convencoes.md)
- [**`ADR-001: Decisões Arquiteturais do Pacote TL.BaseContracts`**](docs/adr/ADR-001-tl-basecontracts.md)
- [**`ADR-002: Decisões Arquiteturais do Pacote TL.HealthCheck`**](docs/adr/ADR-002-tl-healthcheck.md)
- [**`ADR-003: Decisões Arquiteturais do Pacote TL.RabbitMQ`**](docs/adr/ADR-003-tl-rabbitmq.md)
- [**`ADR-004: Decisões Arquiteturais do Pacote TL.Kafka`**](docs/adr/ADR-004-tl-kafka.md)
- [**`ADR-005: Decisões Arquiteturais do Pacote TL.InvokePrivate`**](docs/adr/ADR-005-tl-invokeprivate.md)
- [**`ADR-006: Pipeline HTTP, Resiliência e Padronização de Erros RFC 7807 (TL.MiddlewareLibrary)`**](docs/adr/ADR-006-tl-middlewarelibrary.md)

