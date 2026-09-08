# ADR-004: Decisões Arquiteturais do Pacote TL.Kafka

##  1. Contexto e Motivação

O Apache Kafka é a plataforma padrão da indústria para streaming de eventos de altíssimo volume e ordenação garantida por partição. No entanto, o SDK oficial `Confluent.Kafka` exige tratamento minucioso de configurações (idempotência, ACKs, serialização manual, injeção de headers de telemetria, gerenciamento de offsets e fallback para Dead Letter Topics - DLT).

Sem abstração, cada time criava consumidores com comportamentos divergentes (como auto-commit prematuro que causava perda de mensagens em caso de crash do pod).

##  2. Decisões Arquiteturais

### 2.1. Implementação da Porta Agnóstica `IEventProducer`
- O `KafkaProducer` implementa `IEventProducer` e `IKafkaProducer`.
- **Idempotência Nativa**: Configura `EnableIdempotence = true` e `Acks = Acks.All` por padrão, garantindo que retentativas de rede no broker não dupliquem eventos na partição.
- **Particionamento por Chave**: Suporta `EventMetadata.WithKafkaPartitionKey(key)`, garantindo que mensagens relacionadas (ex: do mesmo cliente ou pedido) sejam roteadas para a mesma partição.
- **Injeção Automática de Headers de Tracing**: Propaga `correlation-id`, `event-type` e `event-id` nos headers do Kafka para rastreamento no OpenTelemetry.

### 2.2. Consumidor Resiliente (`KafkaConsumer<TEvent, THandler>`)
- Implementado como `BackgroundService` gerenciado pelo host.
- **Commit Manual de Offsets (`EnableAutoCommit = false`)**: O offset só é commitado no Kafka após a execução bem-sucedida do `IEventHandler<T>` ou após o desvio seguro para o Dead Letter Topic.
- **Dead Letter Topic (DLT)**: Quando um evento falha após as retentativas do Polly, ele é publicado no tópico `.dlt` antes de efetuar o commit do offset no tópico principal, impedindo o travamento da partição sem perder dados.

### 2.3. Configuração Fluente no DI
- `services.AddKafkaMessaging(config)`
- `services.AddKafkaConsumer<OrderCreatedEvent, OrderCreatedHandler>("events.orders.v1")`

##  3. Consequências e Trade-offs

###  Vantagens:
- Alta confiabilidade sem risco de perda de mensagens ou duplicidade indesejada.
- Facilidade de migração entre Kafka e RabbitMQ apenas alterando o registro no `Program.cs`.


##  4. Status de Verificação
- Coberto por **5 testes unitários** no `CommonHelpers.Kafka.Tests` (100% passing).

