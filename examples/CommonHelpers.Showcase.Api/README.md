# 🌟 CommonHelpers Showcase API

> Projeto executável e vitrine técnica demonstrando a utilização prática e integrada de todos os pacotes da suíte **CommonHelpers** em ASP.NET Core (.NET 8 / .NET 9).

---

##  Como Executar Localmente

### 1. Executando a partir da raiz da solução:

```powershell
dotnet run --project examples/CommonHelpers.Showcase.Api
```

A aplicação iniciará e o **Swagger UI** estará acessível na raiz:
-  **Swagger UI**: [http://localhost:5000](http://localhost:5000) (ou porta configurada)

---

##  O que este Showcase Demonstra?

| Módulo / Pacote | Rota Exposta | Demonstração Prática |
| :--- | :--- | :--- |
| **`RequestResponse`** | `GET /api/orders/{id}` | Busca de pedido por ID com `Result<OrderDto>` e pattern matching `Match()`. |
| **`RequestResponse`** | `POST /api/orders` | Validação de múltiplos campos com `ValidationError` (RFC 7807) e disparo de evento via `IEventProducer`. |
| **`RequestResponse`** | `GET /api/orders` | Paginação padronizada com `PagedRequest` e envelope `PagedResult<OrderDto>`. |
| **`HealthCheck`** | `GET /health` | Diagnóstico de subsistemas (SQL, AMQP, API) formatado para UI Client. |
| **`HealthCheck`** | `GET /liveness` | Sonda de vivacidade para orquestradores (Kubernetes / Docker). |
| **`HealthCheck`** | `GET /ready` | Sonda de prontidão para recebimento de tráfego. |
| **`RabbitMQ / Kafka`** | *Background Event* | Processamento desacoplado de eventos com `OrderCreatedHandler`. |
| **`InvokePrivate`** | `GET /api/diagnostics/reflection-demo` | Invocação controlada de método privado com desembrulho de exceções. |


