using System;
using System.Collections.Generic;
using CommonHelpers.Messaging;
using FluentAssertions;

namespace CommonHelpers.RequestResponse.Tests
{
    public class MessagingContractsTests
    {
        public record OrderPlacedEvent(string OrderId, decimal Amount);

        [Fact]
        public void Given_EventMessage_Create_Should_Populate_Default_Metadata()
        {
            // Arrange
            var payload = new OrderPlacedEvent("ORD-999", 250.50m);

            // Act
            var message = EventMessage<OrderPlacedEvent>.Create(payload, correlationId: "corr-123");

            // Assert
            message.Payload.Should().Be(payload);
            message.CorrelationId.Should().Be("corr-123");
            message.EventId.Should().NotBeEmpty();
            message.EventType.Should().Be(nameof(OrderPlacedEvent));
            message.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
            message.Headers.Should().NotBeNull();
            message.Headers.Should().BeEmpty();
        }

        [Fact]
        public void Given_EventMessage_With_Null_Payload_Should_Throw_ArgumentNullException()
        {
            // Act & Assert
            Action act = () => new EventMessage<OrderPlacedEvent>(
                Guid.NewGuid(), "corr", DateTimeOffset.UtcNow, "type", null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Given_EventMetadata_Fluent_Methods_Should_Chain_Properties()
        {
            // Act
            var metadata = new EventMetadata()
                .WithKafkaPartitionKey("partition-1")
                .WithRabbitMqRoutingKey("orders.v1.placed")
                .WithCorrelationId("trace-789")
                .WithHeader("tenant", "alpha");

            // Assert
            metadata.PartitionKey.Should().Be("partition-1");
            metadata.RoutingKey.Should().Be("orders.v1.placed");
            metadata.CorrelationId.Should().Be("trace-789");
            metadata.Headers.Should().ContainKey("tenant").WhoseValue.Should().Be("alpha");
        }
    }
}

