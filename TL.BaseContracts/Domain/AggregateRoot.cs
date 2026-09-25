using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TL.BaseContracts.Domain
{
    /// <summary>
    /// Representa a raiz de agregação do DDD, responsável por garantir a consistência das entidades filhas e disparar eventos de domínio.
    /// </summary>
    /// <typeparam name="TId">O tipo do identificador da raiz de agregação.</typeparam>
    public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
    {
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

        /// <summary>
        /// Coleção somente leitura dos eventos de domínio acumulados pela raiz de agregação.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Inicializa uma nova raiz de agregação.
        /// </summary>
        protected AggregateRoot()
        {
        }

        /// <summary>
        /// Inicializa uma nova raiz de agregação com seu identificador.
        /// </summary>
        /// <param name="id">O identificador exclusivo da raiz de agregação.</param>
        protected AggregateRoot(TId id) : base(id)
        {
        }

        /// <summary>
        /// Adiciona um novo evento de domínio à lista de eventos pendentes de publicação.
        /// </summary>
        /// <param name="domainEvent">O evento de domínio a ser registrado.</param>
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent != null)
            {
                _domainEvents.Add(domainEvent);
            }
        }

        /// <summary>
        /// Limpa todos os eventos de domínio registrados após sua publicação bem-sucedida.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}

