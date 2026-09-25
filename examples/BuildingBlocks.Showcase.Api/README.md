# 🌟 TL.BuildingBlocks Showcase API

> Projeto executável e vitrine técnica demonstrando a utilização prática e integrada dos pacotes da suíte **TL.BuildingBlocks** (`TL.BaseContracts`, `TL.HealthCheck`, `TL.MiddlewareLibrary`, `TL.Resilience`) em ASP.NET Core (.NET 8 / .NET 9).

---

## 🚀 Como Executar Localmente

### 1. Executando a partir da raiz da solução:

```powershell
dotnet run --project examples/BuildingBlocks.Showcase.Api
```

A aplicação iniciará e o **Swagger UI** estará acessível na raiz:
- 📖 **Swagger UI**: [http://localhost:5000](http://localhost:5000) (ou porta configurada)

---

## 📋 O que este Showcase Demonstra?

| Módulo / Pacote | Rota Exposta | Demonstração Prática |
| :--- | :--- | :--- |
| **`BaseContracts`** | `GET /api/orders/{id}` | Busca de pedido por ID com `Result<OrderDto>` e pattern matching `Match()`. |
| **`BaseContracts`** | `POST /api/orders` | Validação de múltiplos campos com `ValidationError` (RFC 7807) e disparo de evento via `IEventProducer`. |
| **`BaseContracts`** | `GET /api/orders` | Paginação tradicional com `PagedRequest` e envelope `PagedResult<OrderDto>`. |
| **`BaseContracts`** | `GET /api/orders/keyset` | Paginação contínua O(1) por cursor com `SeekRequest<Guid>` e `SeekResult<OrderDto, Guid>`. |
| **`BaseContracts (CQRS)`** | `GET /api/cqrs/orders/{id}` | Desacoplamento via consulta pura com `IQuery` e `IQueryHandler`. |
| **`BaseContracts (CQRS)`** | `POST /api/cqrs/orders` | Processamento de mutação desacoplada com `ICommand` e `ICommandHandler`. |
| **`Resilience`** | `GET /api/resilience/retry-demo` | Execução resiliente com Polly v8, backoff exponencial e jitter decorrelacionado via `ResilienceHelper`. |
| **`HealthCheck`** | `GET /health` | Diagnóstico de subsistemas (SQL, AMQP, API) formatado para UI Client. |
| **`HealthCheck`** | `GET /liveness` | Sonda de vivacidade para orquestradores (Kubernetes / Docker). |
| **`HealthCheck`** | `GET /ready` | Sonda de prontidão para recebimento de tráfego. |
| **`MiddlewareLibrary`** | `GET /api/middlewares/timing` | Medição de latência com zero alocação via cabeçalho `X-Response-Time-Ms`. |
| **`MiddlewareLibrary`** | `GET /api/middlewares/cache` | Cache em memória transparente para requisições GET com cabeçalho `X-Cache` (HIT/MISS). |
| **`MiddlewareLibrary`** | `GET /api/middlewares/validation-error` | Resposta RFC 7807 (400) com dicionário de campos do `ValidationError` de `TL.BaseContracts`. |
| **`MiddlewareLibrary`** | `GET /api/middlewares/not-found-error` | Resposta RFC 7807 (404) para `NotFoundException` via `StatusCodeMiddleware`. |
| **`MiddlewareLibrary`** | `GET /api/middlewares/unhandled-crash` | Fallback global RFC 7807 (500) com rastreabilidade via `X-Correlation-Id`. |
