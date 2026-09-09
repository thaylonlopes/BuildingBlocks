# 🩺 TL.HealthCheck

> Extensões fluentes para configuração padronizada de **Health Checks**, probes de **Liveness** (`/liveness`), **Readiness** (`/ready`) e dashboard com UI (`/health`) em ASP.NET Core e Worker Services (.NET 8 e .NET 9).

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0%20(LTS)-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📦 Instalação

Adicione o pacote ao seu projeto via .NET CLI:

```bash
dotnet add package TL.HealthCheck --version 0.1.0
```

Ou através do Gerenciador de Pacotes do Visual Studio:

```powershell
Install-Package TL.HealthCheck -Version 0.1.0
```

---

## 🚀 Como Usar

### 1. Em Web APIs / Minimal APIs (ASP.NET Core)

Configure os probes padrão no seu `Program.cs`:

```csharp
using TL.HealthCheck;

var builder = WebApplication.CreateBuilder(args);

// 1. Registra os Health Checks padrão (Liveness e Readiness)
builder.Services.AddRequiredHealthChecks();

var app = builder.Build();

// 2. Mapeia automaticamente as rotas /health, /liveness e /ready
app.MapRequiredHealthCheck();

app.Run();
```

### 2. Adicionando Verificações Customizadas (Banco de Dados, Filas, APIs)

```csharp
builder.Services.AddRequiredHealthChecks(() => new[]
{
    new CheckConfig("database", new[] { "ready" }, () => HealthCheckResult.Healthy(), TimeSpan.FromSeconds(2)),
    new CheckConfig("redis_cache", new[] { "ready" }, () => HealthCheckResult.Healthy(), TimeSpan.FromSeconds(1)),
    new CheckConfig("payment_gateway", new[] { "live" }, () => HealthCheckResult.Healthy(), TimeSpan.FromSeconds(5))
});
```

### 3. Em Worker Services (Background Services)

Mesmo em aplicações que rodam apenas em background sem servidor web nativo, você pode acoplar um web host interno para expor os endpoints de diagnóstico para o Kubernetes:

```csharp
using TL.HealthCheck;

IHost host = Host.CreateDefaultBuilder(args)
    .AddWorkerServiceHealthChecks() // Acopla um endpoint web interno para /liveness e /ready
    .ConfigureServices(services =>
    {
        services.AddHostedService<QueueProcessingWorker>();
    })
    .Build();

await host.RunAsync();
```

### 4. Customização de Rotas via `appsettings.json`

Você pode sobrescrever as rotas padrão diretamente no arquivo de configuração:

```json
{
  "HealthCheckConfig": {
    "Timeout": 30,
    "Patterns": {
      "Health": "/custom-health",
      "Liveness": "/custom-liveness",
      "Readiness": "/custom-ready"
    }
  }
}
```

---

## 🏛️ Endpoints Padrão Expostos
- **`/health`**: Resposta detalhada formatada para consumo pelo UI Client.
- **`/liveness`**: Sonda de vivacidade para orquestradores (Kubernetes / Docker).
- **`/ready`**: Sonda de prontidão para recebimento de tráfego.
