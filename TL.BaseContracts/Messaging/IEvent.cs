namespace TL.BaseContracts.Messaging
{
    /// <summary>
    /// Interface marcadora para identificar eventos no ecossistema corporativo.
    /// </summary>
    /// <remarks>
    /// Utilizada como restrição genérica em manipuladores, barramentos de mensageria,
    /// pipelines de medição e tabelas de Outbox Pattern (<c>where TEvent : IEvent</c>).
    /// </remarks>
    public interface IEvent
    {
    }
}

