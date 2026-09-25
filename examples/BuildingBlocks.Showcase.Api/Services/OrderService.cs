using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL.BaseContracts;
using TL.BaseContracts.Messaging;
using BuildingBlocks.Showcase.Api.Events;
using BuildingBlocks.Showcase.Api.Models;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Showcase.Api.Services
{
    /// <summary>
    /// Serviço de domínio para gerenciamento de pedidos demonstrando Result Pattern, ValidationError, PagedResult e Mensageria Agnóstica.
    /// </summary>
    public class OrderService
    {
        private static readonly ConcurrentDictionary<Guid, OrderDto> _database = new();
        private readonly IEventProducer _producer;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IEventProducer producer, ILogger<OrderService> logger)
        {
            _producer = producer;
            _logger = logger;

            if (_database.IsEmpty)
            {
                for (int i = 1; i <= 15; i++)
                {
                    var id = Guid.NewGuid();
                    _database[id] = new OrderDto(
                        Id: id,
                        CustomerEmail: $"cliente{i}@empresa.com",
                        TotalAmount: i * 49.90m,
                        Status: "Concluído",
                        CreatedAt: DateTimeOffset.UtcNow.AddHours(-i));
                }
            }
        }

        /// <summary>
        /// Obtém uma listagem paginada de pedidos usando <see cref="PagedRequest"/> e retornando <see cref="PagedResult{T}"/>.
        /// </summary>
        public PagedResult<OrderDto> GetOrdersPaged(PagedRequest request)
        {
            var allOrders = _database.Values
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            int totalCount = allOrders.Count;
            var pagedItems = allOrders
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return PagedResult<OrderDto>.Create(pagedItems, totalCount, request.PageNumber, request.PageSize);
        }

        /// <summary>
        /// Obtém listagem de pedidos com paginação contínua O(1) baseada em cursor (Seek / Keyset Method).
        /// </summary>
        public SeekResult<OrderDto, Guid?> GetOrdersKeyset(SeekRequest<Guid?> request)
        {
            var orderedList = _database.Values
                .OrderBy(o => o.Id)
                .ToList();

            var query = orderedList.AsEnumerable();
            if (request.LastSeenId.HasValue && request.LastSeenId.Value != Guid.Empty)
            {
                query = query.Where(o => o.Id.CompareTo(request.LastSeenId.Value) > 0);
            }

            var items = query.Take(request.PageSize + 1).ToList();
            bool hasNextPage = items.Count > request.PageSize;
            if (hasNextPage)
            {
                items.RemoveAt(items.Count - 1);
            }

            Guid? nextCursor = hasNextPage && items.Count > 0 ? items[items.Count - 1].Id : null;
            return SeekResult<OrderDto, Guid?>.Create(items, request.PageSize, hasNextPage, nextCursor);
        }

        /// <summary>
        /// Obtém um pedido específico por ID demonstrando o Result Pattern com <see cref="Result{T}"/>.
        /// </summary>
        public Result<OrderDto> GetOrderById(Guid id)
        {
            if (_database.TryGetValue(id, out var order))
            {
                return order;
            }

            return Error.NotFound("Order.NotFound", $"O pedido com identificador '{id}' não foi encontrado no sistema.");
        }

        /// <summary>
        /// Cria um novo pedido, valida seus campos gerando <see cref="ValidationError"/> e publica um evento assíncrono via <see cref="IEventProducer"/>.
        /// </summary>
        public async Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            var validationFailures = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
            {
                validationFailures["CustomerEmail"] = new[] { "O e-mail do cliente é obrigatório e deve ter formato válido." };
            }

            if (request.TotalAmount <= 0)
            {
                validationFailures["TotalAmount"] = new[] { "O valor total do pedido deve ser estritamente maior que zero." };
            }

            if (string.IsNullOrWhiteSpace(request.ItemDescription))
            {
                validationFailures["ItemDescription"] = new[] { "A descrição dos itens não pode ser vazia." };
            }

            if (validationFailures.Count > 0)
            {
                _logger.LogWarning("Falha de validação ao criar pedido: {FailureCount} campos inválidos", validationFailures.Count);
                return new ValidationError(
                    code: "Order.ValidationFailed",
                    message: "Os dados informados para a criação do pedido são inválidos.",
                    errors: validationFailures);
            }

            var newOrder = new OrderDto(
                Id: Guid.NewGuid(),
                CustomerEmail: request.CustomerEmail,
                TotalAmount: request.TotalAmount,
                Status: "Pendente",
                CreatedAt: DateTimeOffset.UtcNow);

            _database[newOrder.Id] = newOrder;

            var orderEvent = new OrderCreatedEvent(newOrder.Id, newOrder.CustomerEmail, newOrder.TotalAmount, newOrder.CreatedAt);
            
            var metadata = EventMetadata.Empty
                .WithCorrelationId(Guid.NewGuid().ToString("N"))
                .WithKafkaPartitionKey(newOrder.CustomerEmail) 
                .WithRabbitMqRoutingKey("orders.created"); 

            var publishResult = await _producer.PublishAsync(orderEvent, metadata, ct).ConfigureAwait(false);

            if (publishResult.IsFailure)
            {
                _logger.LogError("Falha ao publicar evento de pedido criado: {ErrorMessage}", publishResult.Error?.Message);
                
            }

            _logger.LogInformation("Pedido {OrderId} criado com sucesso!", newOrder.Id);
            return newOrder;
        }
    }
}

