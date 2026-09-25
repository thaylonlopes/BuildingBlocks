using System;

namespace TL.BaseContracts.Domain
{
    /// <summary>
    /// Contrato fundamental para eventos de domínio que notificam mudanças relevantes de estado no negócio.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Data e hora em formato UTC em que o evento de domínio ocorreu.
        /// </summary>
        DateTime OccurredOnUtc { get; }
    }
}

