using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Showcase.Api.Models;
using BuildingBlocks.Showcase.Api.Services;
using TL.BaseContracts;
using TL.BaseContracts.CQRS;

namespace BuildingBlocks.Showcase.Api.Cqrs
{
    /// <summary>
    /// Comando CQRS para criação de pedidos no sistema.
    /// </summary>
    public sealed record CreateOrderCommand(string CustomerEmail, decimal TotalAmount, string ItemDescription) : ICommand<Result<OrderDto>>
    {
        public Guid IdRequest { get; } = Guid.NewGuid();
    }

    /// <summary>
    /// Manipulador responsável pelo processamento do comando de criação de pedido.
    /// </summary>
    public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<OrderDto>>
    {
        private readonly OrderService _orderService;

        public CreateOrderCommandHandler(OrderService orderService)
        {
            _orderService = orderService;
        }

        public Task<Result<OrderDto>> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
        {
            var request = new CreateOrderRequest(command.CustomerEmail, command.TotalAmount, command.ItemDescription);
            return _orderService.CreateOrderAsync(request, cancellationToken);
        }
    }

    /// <summary>
    /// Consulta CQRS para obtenção de pedido por identificador exclusivo.
    /// </summary>
    public sealed record GetOrderByIdQuery(Guid Id) : IQuery<Result<OrderDto>>
    {
        public Guid IdRequest { get; } = Guid.NewGuid();
    }

    /// <summary>
    /// Manipulador responsável pelo processamento da consulta de pedido por identificador.
    /// </summary>
    public sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        private readonly OrderService _orderService;

        public GetOrderByIdQueryHandler(OrderService orderService)
        {
            _orderService = orderService;
        }

        public Task<Result<OrderDto>> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
        {
            var result = _orderService.GetOrderById(query.Id);
            return Task.FromResult(result);
        }
    }
}
