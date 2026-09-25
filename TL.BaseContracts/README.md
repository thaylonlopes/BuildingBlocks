# 💎 TL.BaseContracts

> Fundação padronizada e unificada de contratos em BCL pura (Commands, Queries, Events, Results, Errors e Paginações) para aplicações .NET modernas e legadas (.NET Standard 2.0, .NET 8 e .NET 9).

[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-purple.svg)](https://dotnet.microsoft.com/)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20(LTS)-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📦 Instalação

Adicione o pacote ao seu projeto via .NET CLI:

```bash
dotnet add package TL.BaseContracts
```

---

## 🚀 Como Usar

### 1. Paginação Padronizada (`PagedResult<T>` e `PagedRequest`)

Padronize as respostas de listagens e relatórios com cálculo automático de páginas e navegação:

```csharp
using TL.BaseContracts;

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
using TL.BaseContracts;

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
using TL.BaseContracts;

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

### 5. Contratos Agnósticos de Mensageria (`TL.BaseContracts.Messaging`)

Publique e consuma eventos sem acoplar sua camada de domínio ou aplicação a nenhum broker concreto (RabbitMQ, Kafka, Azure Service Bus):

```csharp
using TL.BaseContracts.Messaging;

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
using TL.BaseContracts;
using TL.BaseContracts.Messaging;

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

### 8. Primitivos de Domain-Driven Design (`TL.BaseContracts.Domain`)

Construa entidades ricas, raízes de agregação e objetos de valor com igualdade estrutural em BCL pura:

```csharp
using TL.BaseContracts.Domain;

// Objeto de Valor com igualdade estrutural automática
public class Endereco : ValueObject
{
    public string Logradouro { get; }
    public string Cidade { get; }

    public Endereco(string logradouro, string cidade)
    {
        Logradouro = logradouro;
        Cidade = cidade;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Logradouro;
        yield return Cidade;
    }
}

// Raiz de Agregação com eventos de domínio encapsulados
public class Pedido : AggregateRoot<Guid>
{
    public Pedido(Guid id) : base(id)
    {
        AddDomainEvent(new PedidoCriadoEvent(id));
    }
}
```

---

### 9. Semântica CQRS Desacoplada (`TL.BaseContracts.CQRS`)

Separe comandos e consultas sem acoplamento a bibliotecas pesadas de mediação:

```csharp
using TL.BaseContracts;
using TL.BaseContracts.CQRS;

public class CriarClienteCommand : ICommand<Result<Guid>>
{
    public Guid IdRequest { get; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
}

public class CriarClienteCommandHandler : ICommandHandler<CriarClienteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CriarClienteCommand command, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        return Result.Success(id);
    }
}
```

---

### 10. Paginação Keyset / Seek Method $O(1)$ (`SeekRequest` e `SeekResult<T>`)

Realize navegação contínua por cursor com latência constante independente do volume da tabela:

```csharp
using TL.BaseContracts;

// Requisição com cursor e tamanho de lote
var request = new SeekRequest<long>(lastSeenId: 1050, pageSize: 25);

// Retorno imutável com token da próxima página
var result = SeekResult<PedidoDto, long>.Create(
    items: pedidos,
    pageSize: 25,
    hasNextPage: true,
    nextCursor: 1075);
```

---

### 11. Abstrações de Contexto e Multi-Tenancy (`ICurrentUser` e `ICurrentTenant`)

Permita que as camadas de Domínio e Aplicação acessem os dados de identidade do usuário autenticado e do locatário (tenant) sem se acoplarem ao `HttpContext` ou a bibliotecas de infraestrutura:

```csharp
using TL.BaseContracts.Context;

public class CriarPedidoCommandHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;

    public CriarPedidoCommandHandler(ICurrentUser currentUser, ICurrentTenant currentTenant)
    {
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<Result<Guid>> HandleAsync(CriarPedidoCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure<Guid>(Error.Unauthorized("Auth.Required", "Usuário não autenticado."));

        var tenantId = _currentTenant.TenantId;
        var usuarioId = _currentUser.Id;

        // Processar criação do pedido no contexto do tenant
        return Result.Success(Guid.NewGuid());
    }
}
```

---

## 🛡️ Compatibilidade & Princípios
- **.NET Standard 2.0**, **.NET 8.0** (LTS) e **.NET 9.0**
- **Zero Dependências Externas** (100% BCL Pura).
- **Invariantes Protegidas**: `IsSuccess` é estritamente falso quando existem mensagens de erro.

