using System;

namespace BuildingBlocks.Showcase.Api.Models
{
    /// <summary>
    /// Requisição de criação de um novo pedido.
    /// </summary>
    /// <param name="CustomerEmail">E-mail do cliente solicitante.</param>
    /// <param name="TotalAmount">Valor total do pedido.</param>
    /// <param name="ItemDescription">Descrição dos itens do pedido.</param>
    public record CreateOrderRequest(string CustomerEmail, decimal TotalAmount, string ItemDescription);

    /// <summary>
    /// DTO de representação de um pedido processado.
    /// </summary>
    /// <param name="Id">Identificador único do pedido.</param>
    /// <param name="CustomerEmail">E-mail do cliente.</param>
    /// <param name="TotalAmount">Valor total.</param>
    /// <param name="Status">Status atual do processamento.</param>
    /// <param name="CreatedAt">Data e hora de criação.</param>
    public record OrderDto(Guid Id, string CustomerEmail, decimal TotalAmount, string Status, DateTimeOffset CreatedAt);
}

