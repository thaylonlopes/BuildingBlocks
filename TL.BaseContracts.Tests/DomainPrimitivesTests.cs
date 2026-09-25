using System;
using System.Collections.Generic;
using FluentAssertions;
using TL.BaseContracts.Domain;
using Xunit;

namespace TL.BaseContracts.Tests
{
    public class DomainPrimitivesTests
    {
        private sealed class CustomerIdEntity : Entity<Guid>
        {
            public CustomerIdEntity(Guid id) : base(id)
            {
            }
        }

        private sealed class OrderIdEntity : Entity<Guid>
        {
            public OrderIdEntity(Guid id) : base(id)
            {
            }
        }

        private sealed class OrderCreatedDomainEvent : IDomainEvent
        {
            public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
            public Guid OrderId { get; }

            public OrderCreatedDomainEvent(Guid orderId)
            {
                OrderId = orderId;
            }
        }

        private sealed class OrderAggregate : AggregateRoot<Guid>
        {
            public OrderAggregate(Guid id) : base(id)
            {
            }

            public void RegisterOrderCreation()
            {
                AddDomainEvent(new OrderCreatedDomainEvent(Id));
            }
        }

        private sealed class MoneyValueObject : ValueObject
        {
            public decimal Amount { get; }
            public string Currency { get; }

            public MoneyValueObject(decimal amount, string currency)
            {
                Amount = amount;
                Currency = currency;
            }

            protected override IEnumerable<object?> GetEqualityComponents()
            {
                yield return Amount;
                yield return Currency;
            }
        }

        private sealed class TaggedDocumentValueObject : ValueObject
        {
            public string Title { get; }
            public IReadOnlyList<string> Tags { get; }

            public TaggedDocumentValueObject(string title, IReadOnlyList<string> tags)
            {
                Title = title;
                Tags = tags;
            }

            protected override IEnumerable<object?> GetEqualityComponents()
            {
                yield return Title;
                yield return Tags;
            }
        }

        private sealed class AuditableUser : IAuditableEntity, ISoftDeletable
        {
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
            public string? CreatedBy { get; set; }
            public DateTime? UpdatedAtUtc { get; set; }
            public string? UpdatedBy { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime? DeletedAtUtc { get; set; }
        }

        [Fact]
        public void Given_Entities_With_Same_Id_And_Type_Should_Be_Equal()
        {
            var id = Guid.NewGuid();
            var entity1 = new CustomerIdEntity(id);
            var entity2 = new CustomerIdEntity(id);

            entity1.Equals(entity2).Should().BeTrue();
            (entity1 == entity2).Should().BeTrue();
            (entity1 != entity2).Should().BeFalse();
            entity1.GetHashCode().Should().Be(entity2.GetHashCode());
        }

        [Fact]
        public void Given_Entities_With_Different_Id_Should_Not_Be_Equal()
        {
            var entity1 = new CustomerIdEntity(Guid.NewGuid());
            var entity2 = new CustomerIdEntity(Guid.NewGuid());

            entity1.Equals(entity2).Should().BeFalse();
            (entity1 == entity2).Should().BeFalse();
            (entity1 != entity2).Should().BeTrue();
        }

        [Fact]
        public void Given_Entity_Compared_With_Different_Type_Should_Not_Be_Equal()
        {
            var id = Guid.NewGuid();
            var customer = new CustomerIdEntity(id);
            var order = new OrderIdEntity(id);

            var isDifferentTypeEqual = customer.Equals(order);
            isDifferentTypeEqual.Should().BeFalse();
        }

        [Fact]
        public void Given_Entity_Compared_With_Null_Should_Return_False()
        {
            var customer = new CustomerIdEntity(Guid.NewGuid());
            Entity<Guid>? nullEntity = null;

            customer.Equals(nullEntity).Should().BeFalse();
            (customer == nullEntity).Should().BeFalse();
            (nullEntity == customer).Should().BeFalse();
        }

        [Fact]
        public void Given_AggregateRoot_Should_Record_And_Clear_Domain_Events()
        {
            var orderId = Guid.NewGuid();
            var aggregate = new OrderAggregate(orderId);

            aggregate.DomainEvents.Should().BeEmpty();

            aggregate.RegisterOrderCreation();

            aggregate.DomainEvents.Should().HaveCount(1);
            aggregate.DomainEvents.Should().ContainSingle(e => e is OrderCreatedDomainEvent && ((OrderCreatedDomainEvent)e).OrderId == orderId);

            aggregate.ClearDomainEvents();

            aggregate.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Given_ValueObjects_With_Same_Components_Should_Be_Equal()
        {
            var money1 = new MoneyValueObject(100.50m, "BRL");
            var money2 = new MoneyValueObject(100.50m, "BRL");

            money1.Equals(money2).Should().BeTrue();
            (money1 == money2).Should().BeTrue();
            (money1 != money2).Should().BeFalse();
            money1.GetHashCode().Should().Be(money2.GetHashCode());
        }

        [Fact]
        public void Given_ValueObjects_With_Different_Components_Should_Not_Be_Equal()
        {
            var money1 = new MoneyValueObject(100.50m, "BRL");
            var money2 = new MoneyValueObject(200.00m, "BRL");
            var money3 = new MoneyValueObject(100.50m, "USD");

            (money1 == money2).Should().BeFalse();
            (money1 == money3).Should().BeFalse();
            (money1 != money2).Should().BeTrue();
            money1.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Given_Auditable_And_SoftDeletable_Entity_Should_Expose_Properties()
        {
            var user = new AuditableUser
            {
                CreatedBy = "admin",
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = true,
                DeletedAtUtc = DateTime.UtcNow
            };

            user.CreatedBy.Should().Be("admin");
            user.IsDeleted.Should().BeTrue();
            user.DeletedAtUtc.Should().NotBeNull();
        }

        [Fact]
        public void Given_Transient_Entities_Should_Identify_As_Transient_And_Not_Equal_Other_Transients()
        {
            var transient1 = new CustomerIdEntity(Guid.Empty);
            var transient2 = new CustomerIdEntity(Guid.Empty);
            var persisted = new CustomerIdEntity(Guid.NewGuid());

            transient1.IsTransient().Should().BeTrue();
            transient2.IsTransient().Should().BeTrue();
            persisted.IsTransient().Should().BeFalse();

            (transient1 == transient2).Should().BeFalse();
            transient1.Equals(transient2).Should().BeFalse();

            var sameReference = transient1;
            (transient1 == sameReference).Should().BeTrue();
            transient1.Equals(sameReference).Should().BeTrue();
        }

        [Fact]
        public void Given_ValueObjects_With_Nested_Collections_Should_Support_Deep_Structural_Equality()
        {
            var doc1 = new TaggedDocumentValueObject("Spec", new[] { "architecture", "csharp" });
            var doc2 = new TaggedDocumentValueObject("Spec", new List<string> { "architecture", "csharp" });
            var docWithDifferentOrder = new TaggedDocumentValueObject("Spec", new[] { "csharp", "architecture" });
            var docWithDifferentTags = new TaggedDocumentValueObject("Spec", new[] { "architecture", "fsharp" });

            (doc1 == doc2).Should().BeTrue();
            doc1.Equals(doc2).Should().BeTrue();
            doc1.GetHashCode().Should().Be(doc2.GetHashCode());

            (doc1 == docWithDifferentOrder).Should().BeFalse();
            (doc1 == docWithDifferentTags).Should().BeFalse();
        }
    }
}
