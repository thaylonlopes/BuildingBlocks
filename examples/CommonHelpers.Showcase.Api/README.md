# 🌟 TL.CommonHelpers Showcase API

> Projeto executável e vitrine técnica demonstrando a utilização prática e integrada dos pacotes da suíte **TL.CommonHelpers** (`TL.BaseContracts`, `TL.HealthCheck`, `TL.InvokePrivate`) em ASP.NET Core (.NET 8 / .NET 9).

---

## 🚀 Como Executar Localmente

### 1. Executando a partir da raiz da solução:

```powershell
dotnet run --project examples/CommonHelpers.Showcase.Api
```

A aplicação iniciará e o **Swagger UI** estará acessível na raiz:
- 📖 **Swagger UI**: [http://localhost:5000](http://localhost:5000) (ou porta configurada)

---

## 📋 O que este Showcase Demonstra?

| Módulo / Pacote | Rota Exposta | Demonstração Prática |
| :--- | :--- | :--- |
| **`BaseContracts`** | `GET /api/orders/{id}` | Busca de pedido por ID com `Result<OrderDto>` e pattern matching `Match()`. |
| **`BaseContracts`** | `POST /api/orders` | Validação de múltiplos campos com `ValidationError` (RFC 7807) e disparo de evento via `IEventProducer`. |
| **`BaseContracts`** | `GET /api/orders` | Paginação padronizada com `PagedRequest` e envelope `PagedResult<OrderDto>`. |
| **`HealthCheck`** | `GET /health` | Diagnóstico de subsistemas (SQL, AMQP, API) formatado para UI Client. |
| **`HealthCheck`** | `GET /liveness` | Sonda de vivacidade para orquestradores (Kubernetes / Docker). |
| **`HealthCheck`** | `GET /ready` | Sonda de prontidão para recebimento de tráfego. |
| **`Messaging`** | *Background Event* | Processamento desacoplado de eventos em memória via `OrderCreatedHandler`. |
| **`InvokePrivate`** | `GET /api/diagnostics/reflection-demo` | Invocação controlada de método privado com desembrulho de exceções (marcado como Obsolete). |


