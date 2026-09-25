using System;
using System.Collections;
using System.Collections.Generic;

namespace TL.BaseContracts.Domain
{
    /// <summary>
    /// Representa a classe base para Objetos de Valor (Value Objects) do DDD, caracterizados por imutabilidade e igualdade estrutural profunda.
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

            return AreComponentsEqual(GetEqualityComponents(), other.GetEqualityComponents());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ComputeHashCode(GetEqualityComponents());
        }

        private static bool AreComponentsEqual(IEnumerable<object?> leftComponents, IEnumerable<object?> rightComponents)
        {
            using (var leftEnum = leftComponents.GetEnumerator())
            using (var rightEnum = rightComponents.GetEnumerator())
            {
                while (true)
                {
                    bool hasLeft = leftEnum.MoveNext();
                    bool hasRight = rightEnum.MoveNext();

                    if (hasLeft != hasRight)
                    {
                        return false;
                    }

                    if (!hasLeft)
                    {
                        return true;
                    }

                    if (!AreEqual(leftEnum.Current, rightEnum.Current))
                    {
                        return false;
                    }
                }
            }
        }

        private static bool AreEqual(object? a, object? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            if (a is IEnumerable aSeq && b is IEnumerable bSeq && !(a is string) && !(b is string))
            {
                return AreSequenceEqual(aSeq, bSeq);
            }

            return a.Equals(b);
        }

        private static bool AreSequenceEqual(IEnumerable leftSeq, IEnumerable rightSeq)
        {
            var leftEnum = leftSeq.GetEnumerator();
            var rightEnum = rightSeq.GetEnumerator();
            try
            {
                while (true)
                {
                    bool hasLeft = leftEnum.MoveNext();
                    bool hasRight = rightEnum.MoveNext();

                    if (hasLeft != hasRight)
                    {
                        return false;
                    }

                    if (!hasLeft)
                    {
                        return true;
                    }

                    if (!AreEqual(leftEnum.Current, rightEnum.Current))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                if (leftEnum is IDisposable leftDisp)
                {
                    leftDisp.Dispose();
                }

                if (rightEnum is IDisposable rightDisp)
                {
                    rightDisp.Dispose();
                }
            }
        }

        private static int ComputeHashCode(IEnumerable<object?> components)
        {
            unchecked
            {
                int hash = 17;
                foreach (var component in components)
                {
                    hash = (hash * 31) + ComputeComponentHash(component);
                }
                return hash;
            }
        }

        private static int ComputeComponentHash(object? component)
        {
            if (component is null)
            {
                return 0;
            }

            if (component is IEnumerable seq && !(component is string))
            {
                unchecked
                {
                    int collectionHash = 17;
                    foreach (var item in seq)
                    {
                        collectionHash = (collectionHash * 31) + ComputeComponentHash(item);
                    }
                    return collectionHash;
                }
            }

            return component.GetHashCode();
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
