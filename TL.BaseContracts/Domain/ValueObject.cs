using System;
using System.Collections.Generic;
using System.Linq;

namespace TL.BaseContracts.Domain
{
    /// <summary>
    /// Representa a classe base para Objetos de Valor (Value Objects) do DDD, caracterizados por imutabilidade e igualdade estrutural.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Retorna a sequência ordenada dos componentes atômicos que determinam a igualdade estrutural do objeto de valor.
        /// </summary>
        /// <returns>Uma enumeração contendo os valores que compõem a igualdade.</returns>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ValueObject other && Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(ValueObject? other)
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

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ComputeHashCode(GetEqualityComponents());
        }

        private static int ComputeHashCode(IEnumerable<object?> components)
        {
            unchecked
            {
                int hash = 17;
                foreach (var component in components)
                {
                    hash = (hash * 31) + (component?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }

        /// <summary>
        /// Compara dois objetos de valor para verificar se possuem valores idênticos em todos os seus componentes.
        /// </summary>
        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Compara dois objetos de valor para verificar desigualdade estrutural.
        /// </summary>
        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !(left == right);
        }
    }
}
