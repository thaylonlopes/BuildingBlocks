using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TL.BaseContracts;

namespace TL.BaseContracts.Messaging
{
    /// <summary>
    /// Contrato agnóstico (Port) para publicação de eventos assíncronos no ecossistema de mensageria.
    /// </summary>
    /// <remarks>
    /// Permite que a camada de aplicação e domínio envie mensagens sem nenhum acoplamento com
    /// bibliotecas específicas (RabbitMQ, Kafka, Azure Service Bus, AWS SQS).
    /// </remarks>
    /// <example>
    /// <code>
    /// await _eventProducer.PublishAsync(new OrderCreatedEvent(orderId, total), cancellationToken);
    /// </code>
    /// </example>
    public interface IEventProducer
    {
        /// <summary>
        /// Publica um evento assíncrono em uma única linha de código, inferindo automaticamente o tópico/exchange
        /// por convenção de nomenclatura ou atributo declarativo <see cref="Attributes.TopicAttribute"/>,
        /// e extraindo a chave de partição a partir do atributo <see cref="Attributes.PartitionKeyAttribute"/>.
        /// </summary>
        /// <typeparam name="T">O tipo da mensagem de evento.</typeparam>
        /// <param name="message">A instância da mensagem a ser publicada.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Uma <see cref="Task"/> representando a conclusão da publicação.</returns>
        Task PublishAsync<T>(
            T message,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Publica um evento assíncrono para um tópico ou exchange explicitamente informado,
        /// extraindo a chave de partição a partir do atributo <see cref="Attributes.PartitionKeyAttribute"/>.
        /// </summary>
        /// <typeparam name="T">O tipo da mensagem de evento.</typeparam>
        /// <param name="topicOrExchange">Nome do tópico (Kafka) ou Exchange (RabbitMQ) de destino.</param>
        /// <param name="message">A instância da mensagem a ser publicada.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Uma <see cref="Task"/> representando a conclusão da publicação.</returns>
        Task PublishAsync<T>(
            string topicOrExchange,
            T message,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Publica um evento assíncrono para o broker de mensagens configurado acompanhado de metadados de controle.
        /// </summary>
        /// <typeparam name="T">O tipo da mensagem de evento.</typeparam>
        /// <param name="message">A instância da mensagem a ser publicada.</param>
        /// <param name="metadata">Metadados opcionais para roteamento, partição ou cabeçalhos.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Resultado da operação encapsulado em um <see cref="TL.BaseContracts.Result"/>.</returns>
        Task<TL.BaseContracts.Result> PublishAsync<T>(
            T message,
            EventMetadata? metadata,
            CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Publica um lote de eventos de forma atômica ou otimizada para o broker.
        /// </summary>
        /// <typeparam name="T">O tipo das mensagens do lote.</typeparam>
        /// <param name="messages">A coleção de mensagens a serem publicadas.</param>
        /// <param name="metadata">Metadados opcionais aplicados ao lote.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Resultado da operação encapsulado em um <see cref="TL.BaseContracts.Result"/>.</returns>
        Task<TL.BaseContracts.Result> PublishBatchAsync<T>(
            IEnumerable<T> messages,
            EventMetadata? metadata = null,
            CancellationToken cancellationToken = default) where T : class;
    }
}

