using System;
using FluentAssertions;
using TL.BaseContracts.Messaging;
using TL.BaseContracts.Messaging.Attributes;
using TL.BaseContracts.Messaging.Helpers;
using Xunit;

namespace TL.BaseContracts.Tests
{
    public class EventMetadataExtractorTests
    {
        public class PedidoCriadoEvent : IEvent
        {
            [PartitionKey]
            public Guid PedidoId { get; set; }

            [MessageId]
            public string TransacaoId { get; set; } = string.Empty;

            public decimal Valor { get; set; }
        }

        [Topic("ecommerce.pedidos.faturamento.v1")]
        public class PedidoFaturadoEvent : IIntegrationEvent
        {
            [PartitionKey]
            public Guid PedidoId { get; set; }

            public Guid EventId { get; set; } = Guid.NewGuid();

            public DateTimeOffset OccurredOn { get; set; } = DateTimeOffset.UtcNow;
        }

        public class EventoSemAnotacoes
        {
            public string Descricao { get; set; } = string.Empty;
        }

        public class EventoComDuplaPartitionKey
        {
            [PartitionKey]
            public Guid Id1 { get; set; }

            [PartitionKey]
            public Guid Id2 { get; set; }
        }

        public EventMetadataExtractorTests()
        {
            EventMetadataExtractor.ClearCache();
        }

        [Fact]
        public void DeveExtrairPartitionKeyComSucesso()
        {
            var pedidoId = Guid.NewGuid();
            var evento = new PedidoCriadoEvent { PedidoId = pedidoId };

            var chave = EventMetadataExtractor.ExtractPartitionKey(evento);

            chave.Should().Be(pedidoId.ToString());
        }

        [Fact]
        public void DeveRetornarNullQuandoNaoHouverPartitionKey()
        {
            var evento = new EventoSemAnotacoes { Descricao = "Teste" };

            var chave = EventMetadataExtractor.ExtractPartitionKey(evento);

            chave.Should().BeNull();
        }

        [Fact]
        public void DeveLancarExcecaoQuandoHouverDuplaPartitionKey()
        {
            var evento = new EventoComDuplaPartitionKey { Id1 = Guid.NewGuid(), Id2 = Guid.NewGuid() };

            Action act = () => EventMetadataExtractor.ExtractPartitionKey(evento);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*múltiplas propriedades decoradas com [PartitionKey]*");
        }

        [Fact]
        public void DeveExtrairTopicoDeclarativoViaAtributo()
        {
            var topico = EventMetadataExtractor.GetTopicName<PedidoFaturadoEvent>();

            topico.Should().Be("ecommerce.pedidos.faturamento.v1");
        }

        [Fact]
        public void DeveInferirTopicoEmKebabCaseQuandoNaoHouverAtributo()
        {
            var topico = EventMetadataExtractor.GetTopicName<PedidoCriadoEvent>();

            topico.Should().Be("pedido-criado");
        }

        [Fact]
        public void DeveExtrairMessageIdAnotadoComAtributo()
        {
            var evento = new PedidoCriadoEvent { TransacaoId = "trx-999" };

            var id = EventMetadataExtractor.ExtractMessageId(evento);

            id.Should().Be("trx-999");
        }

        [Fact]
        public void DeveExtrairMessageIdDeIntegrationEvent()
        {
            var evento = new PedidoFaturadoEvent();

            var id = EventMetadataExtractor.ExtractMessageId(evento);

            id.Should().Be(evento.EventId.ToString());
        }

        [Fact]
        public void DeveConfigurarCamposCorporativosEmEventMetadataFluente()
        {
            var metadata = EventMetadata.Empty
                .WithTenantId("tenant-alpha")
                .WithUserId("usr-123")
                .WithCausationId("cmd-456")
                .WithMessageId("msg-789")
                .WithKafkaPartitionKey("part-key")
                .WithRabbitMqRoutingKey("route-key");

            metadata.TenantId.Should().Be("tenant-alpha");
            metadata.UserId.Should().Be("usr-123");
            metadata.CausationId.Should().Be("cmd-456");
            metadata.MessageId.Should().Be("msg-789");
            metadata.PartitionKey.Should().Be("part-key");
            metadata.RoutingKey.Should().Be("route-key");
        }
    }
}

