using System;

namespace BuildingBlocks.Showcase.Api.Events
{
    /// <summary>
    /// Evento de domínio disparado quando um pedido é criado com sucesso.
    /// </summary>
    /// <param name="OrderId">Identificador do pedido.</param>
    /// <param name="CustomerEmail">E-mail do cliente.</param>
    /// <param name="TotalAmount">Valor total.</param>
    /// <param name="CreatedAt">Data de criação.</param>
    public record OrderCreatedEvent(Guid OrderId, string CustomerEmail, decimal TotalAmount, DateTimeOffset CreatedAt);
}

