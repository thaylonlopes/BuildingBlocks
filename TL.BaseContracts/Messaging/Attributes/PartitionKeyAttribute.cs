using System;

namespace TL.BaseContracts.Messaging.Attributes
{
    /// <summary>
    /// Atributo declarativo para indicar a propriedade de um evento que define a chave de particionamento (Partition Key).
    /// </summary>
    /// <remarks>
    /// Utilizado primariamente em streaming distribuído (ex: Apache Kafka, Azure Event Hubs, AWS Kinesis)
    /// para garantir que todas as mensagens associadas à mesma entidade de negócio sejam encaminhadas
    /// rigorosamente para a mesma partição física, preservando a ordem cronológica estrita (FIFO).
    /// </remarks>
    /// <example>
    /// <code>
    /// public class OrderCreatedEvent : IEvent
    /// {
    ///     [PartitionKey]
    ///     public Guid OrderId { get; set; }
    ///     public decimal Total { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PartitionKeyAttribute : Attribute
    {
    }
}

