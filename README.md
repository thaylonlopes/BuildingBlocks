#  CommonHelpers

> Suíte utilitária modular em .NET para acelerar a construção de microsserviços, Web APIs e Worker Services com Result Pattern, Health Checks padronizados e mensageria resiliente (RabbitMQ & Kafka).

[![.NET 6.0](https://img.shields.io/badge/.NET-6.0-purple.svg)](https://dotnet.microsoft.com/)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20(LTS)-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-0.0.1--beta.1-orange.svg)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/Tests-79%2F79%20Passing-brightgreen.svg)]()

---

##  Sumário

- [ O que é o CommonHelpers?](#-o-que-é-o-commonhelpers)
- [ Pacotes NuGet](#-pacotes-nuget)
- [ Guia de Início Rápido](#-guia-de-início-rápido)
  - [1. Mensageria Agnóstica (Ports & Adapters)](#1-mensageria-agnóstica-ports--adapters)
  - [2. Paginação Padronizada e Result Pattern](#2-paginação-padronizada-e-result-pattern)
  - [3. Health Checks Padronizados em 1 Linha](#3-health-checks-padronizados-em-1-linha)
  - [4. Showcase & API Executável](#4--showcase--api-executável)
- [ Executando os Testes](#-executando-os-testes)
- [ Catálogo de Decisões Arquiteturais (ADRs)](#️-catálogo-de-decisões-arquiteturais-adrs)

---

##  O que é o CommonHelpers?

O **CommonHelpers** é uma coleção modular de blocos fundamentais de infraestrutura, comunicação e resiliência projetada para eliminar código boilerplate repetitivo em microsserviços .NET:

1. ** Result Pattern, Paginação & Contratos ([`CommonHelpers.RequestResponse`](CommonHelpers.RequestResponse/README.md))**: `Result<T>` funcional com `ErrorType`, `PagedResult<T>` imutável com cálculo automático de páginas, `ValidationError` por campo e portas agnósticas de mensageria (`IEventProducer`, `IEventHandler<T>`, `EventMessage<T>`). **Zero dependências externas**.
2. ** Health Checks Padronizados ([`CommonHelpers.HealthCheck`](CommonHelpers.HealthCheck/README.md))**: Probes de *Liveness* (`/liveness`), *Readiness* (`/ready`) e dashboard com UI Client (`/health`) em uma única linha, para Web APIs e Worker Services em segundo plano.
3. ** Adaptador RabbitMQ ([`CommonHelpers.RabbitMQ`](CommonHelpers.RabbitMQ/README.md))**: Implementação de `IEventProducer` e `IRabbitMqProducer` com Publisher Confirms, Dead-Letter Queue (`.dlq`) automática e retentativas com Polly.
4. ** Adaptador Apache Kafka ([`CommonHelpers.Kafka`](CommonHelpers.Kafka/README.md))**: Implementação de `IEventProducer` e `IKafkaProducer` com Partition Keys, Dead Letter Topic (`.dlt`), idempotência e headers de telemetria.
5. ** Invocação Segura de Membros Privados ([`CommonHelpers.InvokePrivate`](CommonHelpers.InvokePrivate/README.md))**: Utilitário baseado em Reflection para testes e código legado com desembrulho de `TargetInvocationException`.

---

## Pacotes NuGet

| Pacote NuGet | Versão | Runtimes Suportados | Descrição & Documentação |
| :--- | :---: | :--- | :--- |
| [**`CommonHelpers.RequestResponse`**](CommonHelpers.RequestResponse/README.md) | `0.0.1-beta.1` | `net6.0; net8.0; net9.0` | Result Pattern, Paginação, ValidationError e Portas Agnósticas de Mensageria. |
| [**`CommonHelpers.HealthCheck`**](CommonHelpers.HealthCheck/README.md) | `0.0.1-beta.1` | `net8.0; net9.0` | Probes de Liveness, Readiness, UI Client e Web Host para Workers. |
| [**`CommonHelpers.RabbitMQ`**](CommonHelpers.RabbitMQ/README.md) | `0.0.1-beta.1` | `net6.0; net8.0; net9.0` | Adaptador RabbitMQ com Publisher Confirms e DLQ automática. |
| [**`CommonHelpers.Kafka`**](CommonHelpers.Kafka/README.md) | `0.0.1-beta.1` | `net6.0; net8.0; net9.0` | Adaptador Kafka com Partition Keys, Idempotência e DLT. |
| [**`CommonHelpers.InvokePrivate`**](CommonHelpers.InvokePrivate/README.md) | `0.0.1-beta.1` | `net6.0; net8.0; net9.0` | Reflection segura com desembrulho de exceções para legados. |

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
using CommonHelpers.Messaging;

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
using CommonHelpers.RequestResponse;

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
using CommonHelpers.HealthCheck;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRequiredHealthChecks();

var app = builder.Build();
app.MapRequiredHealthCheck(); // Expõe /health, /liveness e /ready

app.Run();
```

---

### 4. Showcase & API Executável

Criamos um projeto completo e executável em [`examples/CommonHelpers.Showcase.Api`](examples/CommonHelpers.Showcase.Api) com **Swagger UI** demonstrando na prática o uso integrado de todos os 5 pacotes.

Para executar localmente:

```powershell
dotnet run --project examples/CommonHelpers.Showcase.Api
```
Acesse o Swagger interativo em: **`http://localhost:5000`**.

---

##  Executando os Testes

Para rodar todos os **79 testes unitários** da solução:

```powershell
dotnet test
```

---

##  Catálogo de Decisões Arquiteturais (ADRs)

- [**`ADR-000: Arquitetura e Convenções da Suíte CommonHelpers`**](docs/adr/ADR-000-arquitetura-e-convencoes.md)
- [**`ADR-001: Decisões Arquiteturais do Pacote CommonHelpers.RequestResponse`**](docs/adr/ADR-001-commonhelpers-requestresponse.md)
- [**`ADR-002: Decisões Arquiteturais do Pacote CommonHelpers.HealthCheck`**](docs/adr/ADR-002-commonhelpers-healthcheck.md)
- [**`ADR-003: Decisões Arquiteturais do Pacote CommonHelpers.RabbitMQ`**](docs/adr/ADR-003-commonhelpers-rabbitmq.md)
- [**`ADR-004: Decisões Arquiteturais do Pacote CommonHelpers.Kafka`**](docs/adr/ADR-004-commonhelpers-kafka.md)
- [**`ADR-005: Decisões Arquiteturais do Pacote CommonHelpers.InvokePrivate`**](docs/adr/ADR-005-commonhelpers-invokeprivate.md)
