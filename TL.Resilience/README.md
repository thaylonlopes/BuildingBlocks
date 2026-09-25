# 🛡️ TL.Resilience

> Módulo corporativo de resiliência e tolerância a falhas para microsserviços e Web APIs em .NET 8 e .NET 9 baseado em Polly v8.

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20(LTS)-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📑 Sumário

- [🌟 Visão Geral](#-visão-geral)
- [📦 Instalação](#-instalação)
- [🚀 Como Usar](#-como-usar)
  - [1. Execução Resiliente com Backoff Exponencial e Jitter](#1-execução-resiliente-com-backoff-exponencial-e-jitter)
  - [2. Suporte a CancellationToken e ILogger](#2-suporte-a-cancellationtoken-e-ilogger)
  - [3. Resiliência Padronizada em HttpClient (AddStandardResilience)](#3-resiliência-padronizada-em-httpclient-addstandardresilience)
- [🏛️ Políticas Padronizadas](#️-políticas-padronizadas)

---

## 🌟 Visão Geral

O **TL.Resilience** padroniza a implementação de políticas defensivas em chamadas externas e operações assíncronas utilizando a engine moderna do **Polly v8** (`Polly.Core`):

1. **Backoff Exponencial com Decorrelated Jitter**: Evita o efeito manada (*thundering herd*) espalhando temporalmente as retentativas.
2. **Resiliência Integrada em HttpClient**: Extensão `.AddStandardResilience()` para `IHttpClientBuilder` que configura automaticamente retentativas para erros 5xx/408, circuit breaker e timeout.
3. **Cancelamento e Logs Estruturados**: Respeito rigoroso a `CancellationToken` e alertas estruturados via `ILogger`.

---

## 📦 Instalação

Adicione o pacote ao seu projeto via .NET CLI:

```bash
dotnet add package TL.Resilience --version 0.4.0
```

---

## 🚀 Como Usar

### 1. Execução Resiliente com Backoff Exponencial e Jitter

Utilize `ResilienceHelper.ExecuteWithRetryAsync` para proteger chamadas assíncronas contra falhas transitórias:

```csharp
using TL.Resilience;

var resultado = await ResilienceHelper.ExecuteWithRetryAsync(async ct =>
{
    return await servicoExterno.ConsultarDadosAsync(ct);
}, maxRetryAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(200));
```

---

### 2. Suporte a CancellationToken e ILogger

Propague tokens de cancelamento e injete instâncias de telemetria estruturada:

```csharp
using TL.Resilience;

await ResilienceHelper.ExecuteWithRetryAsync(async ct =>
{
    await servicoPagamento.ProcessarTransacaoAsync(dados, ct);
}, 
maxRetryAttempts: 3, 
initialDelay: TimeSpan.FromMilliseconds(300),
logger: _logger,
cancellationToken: cancellationToken);
```

---

### 3. Resiliência Padronizada em HttpClient (`AddStandardResilience`)

No `Program.cs`, registre clientes HTTP protegidos com uma única linha:

```csharp
using TL.Resilience;

builder.Services.AddHttpClient("GatewayPagamentos", client =>
{
    client.BaseAddress = new Uri("https://api.gateway.com");
})
.AddStandardResilience();
```

Essa configuração ativa:
- **Retry Strategy**: 3 tentativas com backoff exponencial e jitter para status 5xx, 408 (Request Timeout), `HttpRequestException` e `TimeoutRejectedException`.
- **Circuit Breaker Strategy**: Interrupção temporária de tráfego se a taxa de falha atingir 50% em amostragem de 30s.
- **Timeout Strategy**: Limite de 30 segundos por tentativa.

---

## 🏛️ Políticas Padronizadas

| Estratégia | Parâmetro Padrão | Comportamento |
| :--- | :--- | :--- |
| **Retry** | 3 tentativas, backoff exponencial + jitter | Trata falhas de rede, 5xx e 408 com espaçamento decorrelacionado |
| **Circuit Breaker** | 50% de falhas, quebra de 15s | Bloqueia chamadas quando o destino estiver consistentemente indisponível |
| **Timeout** | 30 segundos | Evita retenção prolongada de threads e conexões em espera |
