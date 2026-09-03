# ADR-000: Arquitetura e Convenções da Suíte CommonHelpers
## 📋 1. Contexto e Motivação

O ecossistema `CommonHelpers` foi estruturado para resolver o problema de proliferação de código utilitário duplicado, contratos inconsistentes e configurações de infraestrutura fragmentadas entre múltiplos microsserviços corporativos em .NET.

A biblioteca tem como missão fornecer **blocos fundamentais de alta coesão e baixo acoplamento**, garantindo ergonomia para desenvolvedores, robustez contra falhas e integração simplificada em pipelines de nuvem e Kubernetes.

---

## 🎯 2. Decisões Arquiteturais e Invariantes

### 2.1. Empacotamento Granular e Modularidade (1 Pacote = 1 Responsabilidade)
A solução é dividida em pacotes NuGet independentes, evitando dependências transitivas indesejadas em microsserviços consumidores:
- `CommonHelpers.RequestResponse`: Modelos de resultado, erros tipados, paginação e contratos agnósticos de mensageria (**Zero dependências externas**).
- `CommonHelpers.HealthCheck`: Diagnósticos e sondas para ASP.NET Core e Worker Services.
- `CommonHelpers.RabbitMQ`: Adaptador resiliente AMQP com Dead-Letter Queue automática.
- `CommonHelpers.Kafka`: Adaptador resiliente Apache Kafka com Partition Keys e DLT.
- `CommonHelpers.InvokePrivate`: Reflection controlada com desembrulho de exceções para legados.

### 2.2. Multi-Targeting Moderno (.NET 6, .NET 8 e .NET 9)
Todos os pacotes suportam compilação multi-target para as versões modernas e suportadas do ecossistema .NET:
- `<TargetFrameworks>net6.0;net8.0;net9.0</TargetFrameworks>` (RequestResponse, RabbitMQ, Kafka, InvokePrivate).
- `<TargetFrameworks>net8.0;net9.0</TargetFrameworks>` (HealthCheck, devido às dependências do ASP.NET Core 8/9).

### 2.3. Versionamento Semântico e Pré-Lançamento Padronizado
- Versão inicial alinhada em `<Version>0.0.1-beta.1</Version>` para todos os pacotes.
- Inclusão de símbolos de depuração `<SymbolPackageFormat>snupkg</SymbolPackageFormat>` e `<PackageReadmeFile>README.md</PackageReadmeFile>` embarcado em cada `.nupkg`.

### 2.4. Convenções de Código e Engenharia de Qualidade
- **Documentação XML Humanizada**: `<GenerateDocumentationFile>true</GenerateDocumentationFile>` com tags `<summary>`, `<param>`, `<returns>` e `<example>` em 100% das APIs públicas.
- **Proteção por Guard Clauses**: Validações de entrada explícitas (`ArgumentNullException.ThrowIfNull`) e imutabilidade em modelos de dados.
- **TDD e Cobertura**: Todo pacote de produção possui seu respectivo projeto de testes automatizados (`*.Tests`).

---

## ⚖️ 3. Consequências e Trade-offs

### ✅ Vantagens:
- Consumidores instalam apenas os pacotes que necessitam (ex: quem usa apenas RabbitMQ não baixa dependências do Kafka).
- IntelliSense enriquecido e acolhedor para novos desenvolvedores.
- Facilidade de manutenção e evolução contínua sem quebras de compatibilidade inesperadas.

---

## 🗺️ 4. Mapa de ADRs por Projeto

| ADR | Projeto | Foco Arquitetural |
| :--- | :--- | :--- |
| **`ADR-001`** | `CommonHelpers.RequestResponse` | Result Pattern, Paginação, ValidationError e Portas Agnósticas. |
| **`ADR-002`** | `CommonHelpers.HealthCheck` | Sondas de Liveness/Readiness, UI Client e Web Host para Workers. |
| **`ADR-003`** | `CommonHelpers.RabbitMQ` | Adaptador RabbitMQ com Publisher Confirms e DLQ automática. |
| **`ADR-004`** | `CommonHelpers.Kafka` | Adaptador Kafka com Partition Keys, Idempotência e DLT. |
| **`ADR-005`** | `CommonHelpers.InvokePrivate` | Reflection controlada com desembrulho de `TargetInvocationException`. |

