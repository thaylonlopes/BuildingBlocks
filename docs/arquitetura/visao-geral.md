# Visão Geral da Arquitetura — CommonHelpers

##  1. Resumo Executivo

O **CommonHelpers** é uma suíte utilitária modular em C# projetada para fornecer blocos fundamentais de infraestrutura compartilhada, padronização de comunicação e utilitários transversais para microsserviços e bibliotecas corporativas com amplo suporte a runtimes (.NET 6, .NET 8 e .NET 9).

A biblioteca abrange 5 módulos de produção e suítes de testes unitários dedicadas:
1. **Result Pattern, Paginação & Contratos (`TL.BaseContracts`)**: `Result<T>` funcional com `ErrorType`, `PagedResult<T>` imutável, `ValidationError` por campo e portas agnósticas de mensageria (`IEventProducer`, `IEventHandler<T>`, `EventMessage<T>`). **Zero dependências externas**.
2. **Configuração de Health Checks Padronizados (`TL.HealthCheck`)**: Extensões fluentes para configuração de liveness (`/liveness`), readiness (`/ready`) e dashboard (`/health`) com suporte a UI Client e Worker Services.
3. **Adaptador RabbitMQ (`TL.RabbitMQ`)**: Implementação resiliente AMQP de `IEventProducer` e `IRabbitMqProducer` com Publisher Confirms, Dead-Letter Queue (`.dlq`) automática e Polly (migrado para `TL.Messaging`).
4. **Adaptador Apache Kafka (`TL.Kafka`)**: Implementação resiliente de `IEventProducer` e `IKafkaProducer` com Partition Keys para ordenação, idempotência nativa e Dead Letter Topic (`.dlt`) (migrado para `TL.Messaging`).
5. **Invocação Dinâmica de Membros Privados (`TL.InvokePrivate`)**: Utilitário baseado em Reflection para testes e suporte a legados com desembrulho de `TargetInvocationException` (descontinuado).

---

## 🏛️ 2. Diagramas C4

### 2.1 Nível 1: Diagrama de Contexto de Sistema (C4 Context)

```mermaid
graph TD
    Client["Microsserviços, Web APIs & Worker Services<br/>[.NET Standard 2.0 / .NET 8 / .NET 9]"]
    Prometheus["Sistemas de Observabilidade<br/>[Kubernetes / Prometheus / Grafana]"]
    Brokers["Brokers de Mensageria<br/>[RabbitMQ / Apache Kafka]"]

    subgraph CommonHelpersSystem ["CommonHelpers (Suíte Modular Multi-Target)"]
        CH["CommonHelpers Suite<br/>[netstandard2.0 / net8.0 / net9.0]"]
    end

    Client -->|"Consome Result Pattern, Paginação e IEventProducer"| CH
    CH -->|"Publica/Consome eventos assíncronos via"| Brokers
    Prometheus -->|"Sonda probes de liveness e readiness configurados via"| CH
```

### 2.2 Nível 2: Diagrama de Containers (C4 Container)

```mermaid
graph TD
    subgraph "Aplicações Consumidoras"
        AppHost["Host Application<br/>(ASP.NET Core Web API / Worker Service)"]
    end

    subgraph "CommonHelpers - Módulos de Produção"
        RR["TL.BaseContracts<br/>(Result&lt;T&gt;, PagedResult&lt;T&gt;, ValidationError, IEventProducer)<br/>[netstandard2.0, net8.0, net9.0]"]
        HC["TL.HealthCheck<br/>(Liveness, Readiness, UI Client, Worker Host)<br/>[net8.0, net9.0]"]
        RMQ["TL.RabbitMQ<br/>(RabbitMqProducer, DLQ, Polly Retry)<br/>[net6.0, net8.0, net9.0]"]
        KFK["TL.Kafka<br/>(KafkaProducer, PartitionKey, DLT)<br/>[net6.0, net8.0, net9.0]"]
        IP["TL.InvokePrivate<br/>(MethodInvoker com Desembrulho)<br/>[net6.0, net8.0, net9.0]"]
    end

    subgraph "Suítes de Testes Unitários (69 Testes - 100% Passing)"
        RRTests["TL.BaseContracts.Tests (40 testes)"]
        HCTests["TL.HealthCheck.Tests (17 testes)"]
        RMQTests["TL.RabbitMQ.Tests (5 testes)"]
        KFKTests["TL.Kafka.Tests (5 testes)"]
        IPTests["TL.InvokePrivate.Tests (12 testes)"]
    end

    AppHost --> RR
    AppHost --> HC
    AppHost --> RMQ
    AppHost --> KFK
    AppHost --> IP

    RRTests -.->|"Valida"| RR
    HCTests -.->|"Valida"| HC
    RMQTests -.->|"Valida"| RMQ
    KFKTests -.->|"Valida"| KFK
    IPTests -.->|"Valida"| IP
```

---

## 📦 3. Catálogo de Componentes e Projetos

| Projeto | Camada / Área | Target Frameworks | Dependências Nuget | Descrição & Responsabilidade |
| :--- | :--- | :--- | :--- | :--- |
| **`TL.BaseContracts`** | Contratos / Result Pattern | `netstandard2.0`<br/>`net8.0`<br/>`net9.0` | **Zero (BCL pura)** | Result Pattern (`Result<T>`, `ErrorType`), paginação imutável (`PagedResult<T>`, `PagedRequest`), `ValidationError` e portas de mensageria (`IEventProducer`, `IEventHandler<T>`). |
| **`TL.HealthCheck`** | Infra / Observabilidade | `net8.0`<br/>`net9.0` | `AspNetCore.HealthChecks.UI.Client`<br/>`Microsoft.Extensions.Diagnostics.HealthChecks` | Métodos de extensão para probes `/health`, `/ready` e `/liveness` em Web APIs e Worker Services. |
| **`TL.RabbitMQ`** | Infra / Mensageria | `net6.0`<br/>`net8.0`<br/>`net9.0` | `RabbitMQ.Client`<br/>`Polly` | Adaptador AMQP com Publisher Confirms, Dead-Letter Queue (`.dlq`) automática e retentativas com Polly. |
| **`TL.Kafka`** | Infra / Mensageria | `net6.0`<br/>`net8.0`<br/>`net9.0` | `Confluent.Kafka`<br/>`Polly` | Adaptador Apache Kafka com Partition Keys, Idempotência nativa e Dead Letter Topic (`.dlt`). |
| **`TL.InvokePrivate`** | Utilitários / Reflection | `net6.0`<br/>`net8.0`<br/>`net9.0` | **Zero (BCL pura)** | Invocação de métodos privados síncronos/assíncronos com desembrulho de `TargetInvocationException` para legados. |

---

## 🏛️ 4. Catálogo de Decisões Arquiteturais (ADRs)

- [**`ADR-000: Arquitetura e Convenções da Suíte TL`**](../adr/ADR-000-arquitetura-e-convencoes.md)
- [**`ADR-001: Decisões Arquiteturais do Pacote TL.BaseContracts`**](../adr/ADR-001-tl-basecontracts.md)
- [**`ADR-002: Decisões Arquiteturais do Pacote TL.HealthCheck`**](../adr/ADR-002-tl-healthcheck.md)
- [**`ADR-003: Decisões Arquiteturais do Pacote TL.RabbitMQ`**](../adr/ADR-003-tl-rabbitmq.md)
- [**`ADR-004: Decisões Arquiteturais do Pacote TL.Kafka`**](../adr/ADR-004-tl-kafka.md)
- [**`ADR-005: Decisões Arquiteturais do Pacote TL.InvokePrivate`**](../adr/ADR-005-tl-invokeprivate.md)
