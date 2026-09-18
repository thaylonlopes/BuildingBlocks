using System;
using System.Collections.Generic;

namespace TL.BaseContracts.Messaging
{
    /// <summary>
    /// Metadados opcionais para enriquecimento do envio de mensagens e controle de provedores (Kafka, RabbitMQ, etc.).
    /// </summary>
    /// <remarks>
    /// Permite configurar chaves de particionamento, roteamento de tópicos/exchanges e cabeçalhos customizados
    /// sem acoplar a lógica de negócio à biblioteca de infraestrutura específica.
    /// </remarks>
    /// <example>
    /// <code>
    /// var metadata = EventMetadata.Empty
    ///     .WithKafkaPartitionKey("cliente-123")
    ///     .WithRabbitMqRoutingKey("pedidos.criados")
    ///     .WithHeader("tenant-id", "corp-01");
    /// </code>
    /// </example>
    public class EventMetadata
    {
        /// <summary>
        /// Obtém uma nova instância limpa de <see cref="EventMetadata"/>.
        /// </summary>
        public static EventMetadata Empty => new();

        /// <summary>
        /// Chave de roteamento usada primariamente pelo RabbitMQ (Routing Key / Binding Key) ou Kafka (Tópico alternativo).
        /// </summary>
        public string? RoutingKey { get; set; }

        /// <summary>
        /// Chave de particionamento usada primariamente pelo Apache Kafka para garantir ordem estrita de mensagens na mesma partição.
        /// </summary>
        public string? PartitionKey { get; set; }

        /// <summary>
        /// Nome da Exchange específica de destino no RabbitMQ (opcional, sobrescreve o valor padrão configurado).
        /// </summary>
        public string? Exchange { get; set; }

        /// <summary>
        /// Nome do Tópico específico de destino no Kafka (opcional, sobrescreve o valor padrão configurado).
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// Identificador de correlação para rastreamento distribuído (OpenTelemetry / Correlation ID).
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Identificador da mensagem causadora direta deste evento no fluxo assíncrono.
        /// </summary>
        public string? CausationId { get; set; }

        /// <summary>
        /// Identificador exclusivo do inquilino/empresa para ambientes corporativos multi-tenant.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Identificador do usuário ou ator que disparou a ação original.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Identificador exclusivo da mensagem para fins de idempotência e deduplicação.
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Dicionário de cabeçalhos adicionais anexados ao envelope da mensagem.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Define a chave de partição para o Kafka.
        /// </summary>
        /// <param name="partitionKey">Valor da chave de particionamento.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithKafkaPartitionKey(string partitionKey)
        {
            PartitionKey = partitionKey;
            return this;
        }

        /// <summary>
        /// Define a chave de roteamento para o RabbitMQ.
        /// </summary>
        /// <param name="routingKey">Valor da routing key.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithRabbitMqRoutingKey(string routingKey)
        {
            RoutingKey = routingKey;
            return this;
        }

        /// <summary>
        /// Define o identificador de correlação para rastreamento.
        /// </summary>
        /// <param name="correlationId">Identificador de correlação.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
            return this;
        }

        /// <summary>
        /// Define o identificador da mensagem causadora.
        /// </summary>
        /// <param name="causationId">Identificador da mensagem que provocou este evento.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithCausationId(string causationId)
        {
            CausationId = causationId;
            return this;
        }

        /// <summary>
        /// Define o identificador do inquilino para ambientes multi-tenant.
        /// </summary>
        /// <param name="tenantId">Identificador exclusivo do tenant.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithTenantId(string tenantId)
        {
            TenantId = tenantId;
            return this;
        }

        /// <summary>
        /// Define o identificador do usuário autor da ação.
        /// </summary>
        /// <param name="userId">Identificador do usuário.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithUserId(string userId)
        {
            UserId = userId;
            return this;
        }

        /// <summary>
        /// Define o identificador determinístico da mensagem para deduplicação.
        /// </summary>
        /// <param name="messageId">Identificador da mensagem.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithMessageId(string messageId)
        {
            MessageId = messageId;
            return this;
        }

        /// <summary>
        /// Adiciona um cabeçalho customizado à mensagem.
        /// </summary>
        /// <param name="key">Chave do cabeçalho.</param>
        /// <param name="value">Valor do cabeçalho.</param>
        /// <returns>A própria instância para encadeamento fluente.</returns>
        public EventMetadata WithHeader(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                Headers[key] = value ?? string.Empty;
            }
            return this;
        }
    }
}
