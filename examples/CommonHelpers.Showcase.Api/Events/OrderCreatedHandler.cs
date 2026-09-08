using System.Threading;
using System.Threading.Tasks;
using TL.BaseContracts;
using TL.BaseContracts.Messaging;
using Microsoft.Extensions.Logging;

namespace CommonHelpers.Showcase.Api.Events
{
    /// <summary>
    /// Manipulador de evento de negócio executado em segundo plano quando um OrderCreatedEvent é recebido da fila/tópico.
    /// </summary>
    public class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger)
        {
            _logger = logger;
        }

        public Task<Result> HandleAsync(EventMessage<OrderCreatedEvent> eventMessage, CancellationToken cancellationToken = default)
        {
            var payload = eventMessage.Payload;

            _logger.LogInformation(
                " [Showcase Consumer] Evento {EventType} recebido! Pedido: {OrderId} | Cliente: {CustomerEmail} | Valor: R$ {TotalAmount:F2} | CorrelationId: {CorrelationId}",
                eventMessage.EventType,
                payload.OrderId,
                payload.CustomerEmail,
                payload.TotalAmount,
                eventMessage.CorrelationId);

            return Task.FromResult(Result.Success());
        }
    }
}

