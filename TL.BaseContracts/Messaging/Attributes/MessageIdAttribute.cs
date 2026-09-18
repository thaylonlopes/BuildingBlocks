using System;

namespace TL.BaseContracts.Messaging.Attributes
{
    /// <summary>
    /// Atributo declarativo para indicar a propriedade de um evento que fornece o identificador exclusivo da mensagem.
    /// </summary>
    /// <remarks>
    /// Essencial para padrões de Idempotência e Deduplicação determinística no consumidor.
    /// Quando presente, o produtor de mensageria utiliza o valor desta propriedade como o <see cref="EventMessage{T}.EventId"/>,
    /// garantindo que retentativas de rede não gerem IDs aleatórios diferentes para o mesmo evento de negócio.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class PaymentProcessedEvent : IEvent
    /// {
    ///     [MessageId]
    ///     public Guid TransactionId { get; set; }
    ///     public decimal Amount { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MessageIdAttribute : Attribute
    {
    }
}

