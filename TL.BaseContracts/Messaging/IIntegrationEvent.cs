using System;

namespace TL.BaseContracts.Messaging
{
    /// <summary>
    /// Contrato para eventos de integração transmitidos entre fronteiras de microsserviços e contextos delimitados (Bounded Contexts).
    /// </summary>
    /// <remarks>
    /// Garante que todo evento de integração possua identidade única determinística e carimbo de tempo em UTC.
    /// </remarks>
    public interface IIntegrationEvent : IEvent
    {
        /// <summary>
        /// Identificador exclusivo global do evento (UUID/GUID).
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Data e hora em que o evento ocorreu no fuso horário UTC.
        /// </summary>
        DateTimeOffset OccurredOn { get; }
    }
}

