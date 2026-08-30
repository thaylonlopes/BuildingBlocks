# ADR-002: Decisões Arquiteturais do Pacote CommonHelpers.HealthCheck


##  1. Contexto e Motivação

Orquestradores modernos de containers (Kubernetes, Docker Swarm, Azure Container Apps, AWS ECS) dependem criticamente de probes HTTP de diagnóstico para gerenciar o ciclo de vida das aplicações:
- **Liveness Probe**: Verifica se o processo da aplicação está vivo. Se falhar, o container é reiniciado.
- **Readiness Probe**: Verifica se a aplicação está pronta para receber tráfego (conexão com banco de dados, brokers, caches). Se falhar, o tráfego é interrompido temporariamente.
- **Dashboard UI**: Visualização consolidada para monitoramento operacional de infraestrutura.

Configurar esses endpoints manualmente em cada microsserviço ou Worker Service em background gerava dezenas de linhas de boilerplate duplicadas em cada `Program.cs`.


##  2. Decisões Arquiteturais

### 2.1. Métodos de Extensão Fluentes de 1 Linha
- `services.AddRequiredHealthChecks()`: Registra os serviços internos de diagnóstico e as sondas padrão de liveness e readiness.
- `app.MapRequiredHealthCheck()`: Mapeia automaticamente as rotas `/health` (dashboard UI), `/liveness` (sonda viva) e `/ready` (sonda pronta).

### 2.2. Customização de Probes via Configuração e Código
- Suporte a verificações adicionais via `CheckConfig` tipado:
  ```csharp
  builder.Services.AddRequiredHealthChecks(() => new[]
  {
      new CheckConfig("database", new[] { "ready" }, () => HealthCheckResult.Healthy(), TimeSpan.FromSeconds(2)),
      new CheckConfig("redis", new[] { "ready" }, () => HealthCheckResult.Healthy(), TimeSpan.FromSeconds(1))
  });
  ```
- Possibilidade de sobrescrever rotas via `appsettings.json` na seção `HealthCheckConfig`.

### 2.3. Suporte Nativo a Worker Services (Background Services)
- Aplicações que rodam em segundo plano (como consumidores de fila sem servidor web Kestrel nativo) podem acoplar um web host interno de baixo consumo através de:
  ```csharp
  Host.CreateDefaultBuilder(args).AddWorkerServiceHealthChecks().Build();
  ```
  permitindo que o Kubernetes monitore a saúde do Worker através das portas HTTP expostas.


##  3. Consequências e Trade-offs

###  Vantagens:
- Padronização 100% uniforme de rotas e formatos JSON de Health Check em todos os serviços corporativos.
- Integração transparente e sem fricção com orquestradores de nuvem e ferramentas como Prometheus / Datadog.

##  4. Status de Verificação
- Coberto por **17 testes unitários** no `CommonHelpers.HealthCheck.Tests` (100% passing).

