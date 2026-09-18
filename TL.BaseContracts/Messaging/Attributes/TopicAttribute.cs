using System;

namespace TL.BaseContracts.Messaging.Attributes
{
    /// <summary>
    /// Atributo declarativo para definir explicitamente o nome de destino do Tópico (Kafka),
    /// Exchange (RabbitMQ) ou Fila em que o evento deve ser publicado.
    /// </summary>
    /// <remarks>
    /// Quando presente, sobrescreve a convenção de nomenclatura automática em kebab-case
    /// derivada do nome do tipo da classe do evento.
    /// </remarks>
    /// <example>
    /// <code>
    /// [Topic("ecommerce.orders.v1")]
    /// public class OrderCreatedEvent : IEvent
    /// {
    ///     [PartitionKey]
    ///     public Guid OrderId { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class TopicAttribute : Attribute
    {
        /// <summary>
        /// Obtém o nome do tópico, exchange ou destino configurado.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Inicializa uma nova instância de <see cref="TopicAttribute"/> com o nome especificado.
        /// </summary>
        /// <param name="name">O nome do tópico, exchange ou fila.</param>
        /// <exception cref="ArgumentException">Lançada caso o nome seja nulo ou vazio.</exception>
        public TopicAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("O nome do tópico/exchange não pode ser nulo ou vazio.", nameof(name));
            }

            Name = name.Trim();
        }
    }
}

