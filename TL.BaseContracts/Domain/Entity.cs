using System;
using System.Collections.Generic;

namespace TL.BaseContracts.Domain
{
    /// <summary>
    /// Representa a classe base para entidades de domínio com identificador fortemente tipado.
    /// </summary>
    /// <typeparam name="TId">O tipo do identificador exclusivo da entidade.</typeparam>
    public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
    {
        /// <summary>
        /// Identificador exclusivo da entidade.
        /// </summary>
        public TId Id { get; protected set; } = default!;

        /// <summary>
        /// Inicializa uma nova instância da entidade.
        /// </summary>
        protected Entity()
        {
        }

        /// <summary>
        /// Inicializa uma nova instância da entidade com seu identificador.
        /// </summary>
        /// <param name="id">O identificador exclusivo da entidade.</param>
        protected Entity(TId id)
        {
            Id = id;
        }

        /// <summary>
        /// Indica se a entidade é transiente (não persistida e com identificador padrão não atribuído).
        /// </summary>
        /// <returns>True se a entidade for transiente; caso contrário, false.</returns>
        public bool IsTransient()
        {
            return EqualityComparer<TId>.Default.Equals(Id, default!);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Entity<TId> other && Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(Entity<TId>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            if (IsTransient() || other.IsTransient())
            {
                return false;
            }

            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (IsTransient())
            {
                return base.GetHashCode();
            }

            return EqualityComparer<TId>.Default.GetHashCode(Id);
        }

        /// <summary>
        /// Compara duas instâncias de entidade para verificar igualdade de identidade.
        /// </summary>
        public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Compara duas instâncias de entidade para verificar desigualdade de identidade.
        /// </summary>
        public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        {
            return !(left == right);
        }
    }
}
