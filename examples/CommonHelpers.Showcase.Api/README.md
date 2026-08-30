# 🌟 CommonHelpers Showcase API

> Projeto de exemplo executável e vitrine técnica demonstrando a utilização prática e integrada de todos os pacotes da suíte **CommonHelpers** em ASP.NET Core (.NET 8 / .NET 9).

---

## 🚀 Como Executar Localmente

### 1. Pré-requisitos
- .NET 8.0 SDK ou .NET 9.0 SDK instalado.

### 2. Rodando a Aplicação
Navegue até a pasta do projeto e execute:

```powershell
dotnet run
```

A aplicação iniciará e o **Swagger UI** estará acessível diretamente na raiz:
- 🌐 **Swagger UI**: [http://localhost:5000](http://localhost:5000) (ou porta configurada)

---

## 🧭 O que este Showcase Demonstra?

### 1. 💎 Result Pattern & ValidationError (`CommonHelpers.RequestResponse`)
- **`GET /api/orders/{id}`**: Utiliza `Result<OrderDto>` e pattern matching `result.Match()` para retornar `200 OK` em caso de sucesso ou `404 Not Found` caso o pedido não exista.
- **`POST /api/orders`**: Valida múltiplos campos e retorna um `ValidationError` caso os dados estejam incorretos.

### 2. 📄 Paginação Padronizada (`PagedResult<T>` e `PagedRequest`)
- **`GET /api/orders?pageNumber=1&pageSize=5`**: Recebe os parâmetros de paginação e retorna o envelope imutável `PagedResult<OrderDto>` contendo contagem total, páginas totais, `hasPreviousPage` e `hasNextPage`.

### 3. 🩺 Health Checks Padronizados (`CommonHelpers.HealthCheck`)
- **`GET /health`**: Resposta formatada para o UI Client com status de cada subsistema.
- **`GET /liveness`**: Sonda de sobrevivência para orquestradores (Kubernetes).
- **`GET /ready`**: Sonda de prontidão para receber tráfego.

### 4. 📬 Mensageria Agnóstica (Ports & Adapters)
- A camada de serviço consome apenas a interface agnóstica `IEventProducer`, permitindo alternar entre **RabbitMQ** e **Apache Kafka** alterando apenas a injeção no `Program.cs`.

### 5. 🔍 Invocação Segura de Reflection (`CommonHelpers.InvokePrivate`)
- **`GET /api/diagnostics/reflection-demo`**: Demonstra a execução de métodos privados com desembrulho automático de exceções.

