# ADR-001: Decisões Arquiteturais do Pacote TL.BaseContracts

## 📋 1. Contexto e Motivação

Em arquiteturas de microsserviços e Web APIs, o tratamento de fluxos de sucesso e falha através de exceções (`throw new Exception(...)`) causa severa degradação de performance por alocações massivas no heap e captura de stack traces custosos, além de obscurecer os fluxos de negócio.

Adicionalmente, problemas comuns como **paginação** de listagens e **agrupamento de múltiplos erros de validação por campo** (FluentValidation / RFC 7807 ProblemDetails) eram reimplementados de forma dispersa e inconsistente em cada projeto.

Na versão `0.2.0`, o pacote passou por um rebranding oficial de `TL.RequestResponse` para **`TL.BaseContracts`**, unificando contratos canônicos de entrada, saída e domínio em BCL pura com multi-targeting (`netstandard2.0;net8.0;net9.0`).

## 🎯 2. Decisões Arquiteturais

### 2.1. Adoção Canônica do Result Pattern
- Implementação das classes imutáveis `Result<T>` e `Result` (não-genérico).
- **Semântica de Erros Ricos**: `record Error` com categorização tipada (`ErrorType: Failure, Validation, NotFound, Conflict, Unauthorized, Forbidden`).
- **Ergonomia Funcional**: Suporte a Pattern Matching via métodos `Match` e transformações via `Map`.
- **Conversão Implícita Segura**:
  - Conversão implícita de `T` para `Result<T>` (sucesso).
  - Conversão implícita de `Error` para `Result<T>` e `Result` (falha).

### 2.2. Paginação Padronizada Imutável (`PagedResult<T>` e `PagedRequest`)
- `PagedRequest`: Contrato que normaliza automaticamente parâmetros de consulta (`PageNumber >= 1`, `PageSize >= 1` com teto configurável de 100), prevenindo divisão por zero em queries de banco.
- `PagedResult<T>`: Estrutura imutável contendo a lista imutável `Items`, `PageNumber`, `PageSize`, `TotalCount`, cálculo exato de `TotalPages` e propriedades booleanas `HasPreviousPage` e `HasNextPage`.

### 2.3. Erros de Validação Detalhados por Campo (`ValidationError`)
- Especialização de `Error` contendo `IReadOnlyDictionary<string, string[]> Errors`.
- Permite mapear perfeitamente os erros de validação de formulários e DTOs, serializando de forma transparente para o formato RFC 7807 (`ValidationProblemDetails`).

### 2.4. Contratos Agnósticos de Mensageria (Ports & Adapters)
- Fornece no namespace `TL.BaseContracts.Messaging` os contratos base:
  - `IEventProducer`: Porta agnóstica para publicação de eventos.
  - `IEventHandler<T>`: Porta agnóstica para consumo de eventos.
  - `EventMessage<T>`: Envelope padronizado imutável (EventId, CorrelationId, Timestamp, EventType, Payload, Headers).
  - `EventMetadata`: Metadados fluentes para enriquecimento contextual.

### 2.5. Interoperabilidade e Rastreabilidade
- `IRequest`: Interface com `Guid IdRequest` para rastreamento de ponta a ponta.
- `Response<T>`: Envelope tradicional mantido com métodos de ponte `ToResult()` e `ToResponse()`.

## ⚖️ 3. Consequências e Trade-offs

### ✅ Vantagens:
- **Zero Dependências**: Pode ser instalado em qualquer biblioteca de domínio ou aplicação sem poluir dependências (100% BCL pura).
- **Previsibilidade**: Elimina bugs de "falha silenciosa" e engolimento de exceções.
- **Produtividade**: Paginação e validação resolvidas em uma linha de código em todas as APIs.

## 🧪 4. Status de Verificação
- Coberto por **40 testes unitários** no `TL.BaseContracts.Tests` (100% passing).

