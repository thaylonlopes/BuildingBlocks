using System;

namespace TL.BaseContracts
{
    /// <summary>
    /// Contrato para requisições de paginação baseada em cursor e keyset (Seek Method) para consultas em tempo constante O(1).
    /// </summary>
    /// <typeparam name="TKey">O tipo da chave de cursor utilizada para referenciar o último registro lido.</typeparam>
    public class SeekRequest<TKey> : IRequest
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        private int _pageSize = DefaultPageSize;

        /// <summary>
        /// Identificador exclusivo da requisição para rastreabilidade ponta a ponta.
        /// </summary>
        public Guid IdRequest { get; } = Guid.NewGuid();

        /// <summary>
        /// O identificador ou chave do último elemento lido na iteração anterior.
        /// </summary>
        public TKey? LastSeenId { get; set; }

        /// <summary>
        /// Quantidade de itens solicitados para a página (padrão: 10, máximo: 100).
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value <= 0)
                {
                    _pageSize = DefaultPageSize;
                }
                else if (value > MaxPageSize)
                {
                    _pageSize = MaxPageSize;
                }
                else
                {
                    _pageSize = value;
                }
            }
        }

        /// <summary>
        /// Inicializa uma nova requisição de paginação por cursor com valores padrão.
        /// </summary>
        public SeekRequest()
        {
        }

        /// <summary>
        /// Inicializa uma nova requisição de paginação por cursor especificando o último elemento lido e o tamanho da página.
        /// </summary>
        /// <param name="lastSeenId">O identificador do último elemento lido.</param>
        /// <param name="pageSize">Quantidade de registros desejados (mínimo 1, máximo 100).</param>
        public SeekRequest(TKey? lastSeenId, int pageSize = DefaultPageSize)
        {
            LastSeenId = lastSeenId;
            PageSize = pageSize;
        }
    }

    /// <summary>
    /// Contrato não-genérico para requisições de paginação por cursor utilizando tokens opacos em formato string.
    /// </summary>
    public class SeekRequest : SeekRequest<string>
    {
        /// <summary>
        /// Inicializa uma nova requisição com valores padrão.
        /// </summary>
        public SeekRequest()
        {
        }

        /// <summary>
        /// Inicializa uma nova requisição com cursor em string e tamanho de página.
        /// </summary>
        /// <param name="lastSeenId">O token de cursor do último registro lido.</param>
        /// <param name="pageSize">Quantidade de itens solicitados.</param>
        public SeekRequest(string? lastSeenId, int pageSize = 10) : base(lastSeenId, pageSize)
        {
        }
    }
}

