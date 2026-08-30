# ADR-003: Decisões Arquiteturais do Pacote CommonHelpers.RabbitMQ

##  1. Contexto e Motivação

O RabbitMQ é amplamente utilizado em arquiteturas orientadas a eventos para mensageria assíncrona baseada no protocolo AMQP. No entanto, sua utilização direta via SDK `RabbitMQ.Client` exige código complexo e repetitivo de gerenciamento de canais (`IModel`), topologia de filas, Publisher Confirms e tratamento de falhas com Dead-Letter Queue (DLQ).

Sem uma camada padronizada, cada serviço implementava sua própria lógica de retry, gerando perda silenciosa de mensagens ou filas bloqueadas por poison messages.


##  2. Decisões Arquiteturais

### 2.1. Implementação da Porta Agnóstica `IEventProducer`
- O `RabbitMqProducer` implementa `IEventProducer` (permitindo injeção desacoplada) e a interface especializada `IRabbitMqProducer` (para publicações diretas com exchange/routingKey customizadas).
- **Publisher Confirms**: Ativado por padrão (`ConfirmSelect()`), garantindo confirmação síncrona/assíncrona de entrega do broker antes do retorno do método.
- **Serialização UTF-8 / JSON**: Serializa automaticamente no envelope `EventMessage<T>` e preenche os metadados AMQP (`BasicProperties.ContentType = "application/json"`, `CorrelationId`, `Type`, `Timestamp`).

### 2.2. Consumidor Resiliente em Background (`RabbitMqConsumer<TEvent, THandler>`)
- Implementado como `BackgroundService` gerenciado pelo runtime do .NET.
- **Declaração Automática de Topologia**:
  1. Cria a Exchange principal (`amq.topic` ou customizada).
  2. Cria a Dead-Letter Exchange (`.dlx`) e a fila Dead-Letter (`.dlq`).
  3. Cria a fila principal com argumentos `x-dead-letter-exchange` apontando para a `.dlx`.
- **Retentativas Inteligentes com Polly**: Aplica retentativas com backoff exponencial antes de rejeitar com NACK (`requeue: false`), enviando a mensagem automaticamente para a `.dlq`.
- **Delegação Limpa para `IEventHandler<T>`**: Executa o manipulador de negócio dentro de um escopo de injeção de dependência (`IServiceScope`).

### 2.3. Configuração Fluente no DI
- `services.AddRabbitMqMessaging(config)`
- `services.AddRabbitMqConsumer<OrderCreatedEvent, OrderCreatedHandler>("app.orders.created", "orders.created")`


##  3. Consequências e Trade-offs

###  Vantagens:
- Zero esforço para configurar topologias robustas com proteção contra poison messages.
- Desacoplamento completo entre a regra de negócio e o RabbitMQ.


##  4. Status de Verificação
- Coberto por **5 testes unitários** no `CommonHelpers.RabbitMQ.Tests` (100% passing).

