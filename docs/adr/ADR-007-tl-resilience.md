# ADR 007: Resiliência Padronizada com Polly v8 (TL.Resilience)

## 🎯 Contexto e Desafio Prático

Em arquiteturas orientadas a microsserviços e sistemas distribuídos, chamadas de rede externas e integrações HTTP entre serviços estão sujeitas a falhas transitórias (quedas momentâneas, latências anômalas, timeouts de gateway e reinicializações de pods).

Historicamente, observava-se:
- Implementações pontuais e inconsistentes de retentativas espalhadas pelas regras de negócio.
- Retentativas em intervalos fixos ou imediatos gerando **efeito manada** (*thundering herd problem*), degradando ainda mais serviços já combalidos.
- Ausência de disjuntores (*Circuit Breaker*), mantendo requisições em cascata e exaurindo pools de conexões e threads.
- Falta de integração transparente com `CancellationToken` e telemetria estruturada.

Com o advento do **Polly v8**, o modelo de composição de resiliência foi completamente reformulado em torno de `ResiliencePipeline` de alta performance, zero alocações desnecessárias e design unificado.

---

## 💡 Decisões Arquiteturais Consolidadas

### 1. Adoção da Engine Moderna Polly v8 (`Polly.Core 8.4.1`)
Adotamos o pacote de núcleo `Polly.Core` como fundação de resiliência corporativa, eliminando dependências legadas do ecossistema Polly v7 e padronizando a composição de estratégias via `ResiliencePipelineBuilder`.

---

### 2. Backoff Exponencial com Jitter Decorrelacionado (`ResilienceHelper`)
Para chamadas e blocos de código assíncronos genéricos, fornecemos `ResilienceHelper.ExecuteWithRetryAsync`:
- **Algoritmo**: `DelayBackoffType.Exponential` com `UseJitter = true`.
- **Efeito**: Espalha uniformemente os momentos de repetição através de números pseudorrandômicos correlacionados ao intervalo base, eliminando picos de sincronização sob carga concorrente.
- **Cancelamento**: Respeita rigorosamente o `CancellationToken`, interrompendo o ciclo defensivo de imediato caso a operação seja abortada.
- **Telemetria**: Log estruturado de nível `Warning` via `ILogger` a cada tentativa acionada.

---

### 3. Resiliência Padronizada para `HttpClient` (`AddStandardResilience`)
Para consumo de APIs REST e clientes HTTP gerenciados, criamos o método de extensão fluente `.AddStandardResilience()` em `IHttpClientBuilder`:

```mermaid
graph LR
    Req(["🌐 Requisição HTTP"]) --> TO["⏱️ Timeout Strategy<br/>(30s por tentativa)"]
    TO --> CB["⚡ Circuit Breaker<br/>(50% falhas / 15s break)"]
    CB --> RT["🔄 Retry Strategy<br/>(3x Exponencial + Jitter)"]
    RT --> Srv["🏢 Serviço de Destino"]
```

1. **Timeout Strategy**: Limite de 30 segundos por tentativa, prevenindo que sockets fiquem pendentes indefinidamente.
2. **Circuit Breaker Strategy**: Quebra automática de 15 segundos ao detectar 50% de falhas em uma janela de 30 segundos com amostragem mínima.
3. **Retry Strategy**: 3 repetições automáticas para status HTTP transitórios (5xx Server Error, 408 Request Timeout) e falhas de transporte (`HttpRequestException`, `TimeoutRejectedException`).

---

### 4. Zero Comentários Inline e Nomes Expressivos
Seguindo as diretrizes de Clean Code do `TL.BuildingBlocks`:
- Nenhuma linha de comentário `//` é admitida no código C#.
- Documentação pública via XML Docs (`/// <summary>`).
- Extração contínua de métodos utilitários expressivos (`IsTransientHttpFailure`, `ConfigureRetryOptions`, `AppendCorrelationIdHeader`).

---

## ⚖️ Consequências e Trade-offs

### Impactos Positivos
- **Tolerância a Falhas Consistente**: Todo microsserviço que referencia `TL.Resilience` herda políticas testadas e comprovadas.
- **Proteção dos Serviços Alvo**: Jitter decorrelacionado e circuit breaker protegem a infraestrutura contra sobrecargas induzidas.
- **Simplicidade de Consumo**: Configuração em uma única linha no setup de injeção de dependência (`AddStandardResilience`).

### Mitigações
- **Idempotência em Métodos Não Seguros**: Requisições HTTP não-idempotentes (`POST`, `PATCH`) devem ter atenção especial ao utilizar retry automático em caso de timeouts, cabendo aos serviços remotos suportar chaves de idempotência quando aplicável.
