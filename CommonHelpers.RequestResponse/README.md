# 💎 CommonHelpers.RequestResponse

> Modelagem moderna de resultados com **Result Pattern**, tipagem semântica de erros com **ErrorType**, erros de validação por campo (**ValidationError**), paginação padronizada (**PagedResult&lt;T&gt;** / **PagedRequest**), rastreabilidade de requisições com **IRequest** e envelope tradicional **Response&lt;T&gt;** com zero dependências externas para .NET 6, .NET 8 e .NET 9.

[![.NET 6.0](https://img.shields.io/badge/.NET-6.0-purple.svg)](https://dotnet.microsoft.com/)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20(LTS)-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-0.0.1--beta.1-orange.svg)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

##  Instalação

Adicione o pacote ao seu projeto via .NET CLI:

```bash
dotnet add package CommonHelpers.RequestResponse --version 0.0.1-beta.1
```

---

##  Como Usar

### 1. Paginação Padronizada (`PagedResult<T>` e `PagedRequest`)

Padronize as respostas de listagens e relatórios com cálculo automático de páginas e navegação:

```csharp
using CommonHelpers.RequestResponse;

[HttpGet("users")]
public async Task<IActionResult> GetUsers([FromQuery] PagedRequest request)
{
    // PagedRequest normaliza automaticamente PageNumber e PageSize
    var (users, totalCount) = await _userRepository.GetPagedAsync(request.PageNumber, request.PageSize);

    // Cria o PagedResult imutável com cálculo automático de páginas
    var pagedResult = PagedResult<UserDto>.Create(users, totalCount, request.PageNumber, request.PageSize);

    return Ok(pagedResult);
    // Retorna: { items: [...], pageNumber: 1, pageSize: 10, totalCount: 45, totalPages: 5, hasPreviousPage: false, hasNextPage: true }
}
```

---

### 2. Múltiplos Erros de Validação por Campo (`ValidationError`)

Integração limpa com FluentValidation, DataAnnotations e formato RFC 7807 (`ProblemDetails`):

```csharp
using CommonHelpers.RequestResponse;

public Result<UserDto> CreateUser(CreateUserCommand command)
{
    var failures = new Dictionary<string, string[]>
    {
        { "Email", new[] { "E-mail é obrigatório.", "Formato inválido." } },
        { "Password", new[] { "A senha deve conter no mínimo 8 caracteres." } }
    };

    if (failures.Count > 0)
    {
        return ValidationError.FromFailures(failures);
    }

    return new UserDto { Email = command.Email };
}
```

---

### 3. Result Pattern Moderno (`Result<T>` e `Error`)

Substitua exceções de fluxo por código funcional limpo, expressivo e seguro:

```csharp
using CommonHelpers.RequestResponse;

public class UserService
{
    public Result<UserDto> RegisterUser(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ValidationError.ForField("Email", "O e-mail fornecido é inválido.");
        }

        var user = new UserDto { Id = Guid.NewGuid(), Email = email };
        return user; // Conversão implícita de sucesso
    }
}
```

### 4. Consumindo com Pattern Matching (`Match`)

```csharp
var result = userService.RegisterUser("dev@empresa.com", "senha123");

string responseText = result.Match(
    onSuccess: user => $"Usuário registrado com sucesso! ID: {user.Id}",
    onFailure: error => $"Falha [{error.Code}] ({error.Type}): {error.Message}"
);
```

### 5. Contratos Agnósticos de Mensageria (`CommonHelpers.Messaging`)

Publique e consuma eventos sem acoplar sua camada de domínio ou aplicação a nenhum broker concreto (RabbitMQ, Kafka, Azure Service Bus):

```csharp
using CommonHelpers.Messaging;

public class OrderService
{
    private readonly IEventProducer _eventProducer;

    public OrderService(IEventProducer eventProducer) => _eventProducer = eventProducer;

    public async Task CreateOrderAsync(OrderDto order, CancellationToken ct)
    {
        var orderCreated = new OrderCreatedEvent(order.Id, order.Total);
        
        // Publicação agnóstica com metadados opcionais
        var metadata = new EventMetadata().WithCorrelationId(Guid.NewGuid().ToString());
        await _eventProducer.PublishAsync(orderCreated, metadata, ct);
    }
}
```

Implementação de Handlers desacoplados:

```csharp
using CommonHelpers.Messaging;
using CommonHelpers.RequestResponse;

public class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task<Result> HandleAsync(EventMessage<OrderCreatedEvent> message, CancellationToken ct)
    {
        Console.WriteLine($"Processando pedido: {message.Payload.OrderId}, EventId: {message.EventId}");
        return Result.Success();
    }
}
```

---

### 6. Rastreabilidade com `IRequest`

```csharp
public class ProcessPaymentCommand : IRequest
{
    public Guid IdRequest { get; } = Guid.NewGuid();
    public decimal Amount { get; set; }
}
```

---

### 7. Interoperabilidade com `Response<T>` (Legado)

```csharp
// Result<T> -> Response<T>
Response<UserDto> legacyResponse = result.ToResponse();

// Response<T> -> Result<T>
Result<UserDto> modernResult = legacyResponse.ToResult();
```

---

##  Compatibilidade & Princípios
- **.NET 6.0**, **.NET 8.0** (LTS) e **.NET 9.0**
- **Zero Dependências Externas** (100% BCL Pura).
- **Invariantes Protegidas**: `IsSuccess` é estritamente falso quando existem mensagens de erro.
