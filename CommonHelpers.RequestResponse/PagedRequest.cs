using System;

namespace CommonHelpers.RequestResponse
{
    /// <summary>
    /// Contrato padronizado para requisições de paginação em consultas de listagem e relatórios.
    /// </summary>
    /// <remarks>
    /// Normaliza automaticamente valores inválidos (como páginas negativas ou zero) para valores padrão seguros,
    /// prevenindo divisão por zero e comportamentos inesperados em queries SQL ou NoSQL.
    /// </remarks>
    /// <example>
    /// <code>
    /// var request = new PagedRequest(pageNumber: 2, pageSize: 20);
    /// </code>
    /// </example>
    public class PagedRequest : IRequest
    {
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        private int _pageNumber = DefaultPageNumber;
        private int _pageSize = DefaultPageSize;

        /// <summary>
        /// Identificador exclusivo da requisição para rastreamento ponta a ponta.
        /// </summary>
        public Guid IdRequest { get; } = Guid.NewGuid();

        /// <summary>
        /// Número da página solicitada (inicia em 1).
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value <= 0 ? DefaultPageNumber : value;
        }

        /// <summary>
        /// Quantidade de itens por página (padrão: 10, máximo: 100).
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
        /// Inicializa uma nova requisição paginada com os valores padrão (Página 1, Tamanho 10).
        /// </summary>
        public PagedRequest()
        {
        }

        /// <summary>
        /// Inicializa uma nova requisição paginada especificando a página e a quantidade de itens.
        /// </summary>
        /// <param name="pageNumber">Número da página desejada (mínimo 1).</param>
        /// <param name="pageSize">Quantidade de itens por página (mínimo 1, máximo 100).</param>
        public PagedRequest(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}

