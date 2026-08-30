using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CommonHelpers.RequestResponse
{
    /// <summary>
    /// Encapsula o resultado de uma consulta paginada com metadados de navegação e totalizadores.
    /// </summary>
    /// <typeparam name="T">O tipo dos itens retornados na lista paginada.</typeparam>
    /// <remarks>
    /// Estrutura imutável que calcula automaticamente o total de páginas (<see cref="TotalPages"/>),
    /// indicador de página anterior (<see cref="HasPreviousPage"/>) e próxima página (<see cref="HasNextPage"/>).
    /// </remarks>
    /// <example>
    /// <code>
    /// var pagedResult = PagedResult&lt;UserDto&gt;.Create(users, totalCount: 100, pageNumber: 1, pageSize: 10);
    /// </code>
    /// </example>
    public class PagedResult<T>
    {
        private static readonly IReadOnlyList<T> EmptyList = new ReadOnlyCollection<T>(new List<T>());

        /// <summary>
        /// Lista imutável contendo os itens da página atual.
        /// </summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// Número da página atual (inicia em 1).
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Quantidade de itens por página.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Quantidade total de registros existentes no banco de dados para a consulta.
        /// </summary>
        public long TotalCount { get; }

        /// <summary>
        /// Quantidade total de páginas calculada com base no <see cref="TotalCount"/> e <see cref="PageSize"/>.
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Indica se existe uma página anterior disponível para navegação.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indica se existe uma próxima página disponível para navegação.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Inicializa uma nova instância imutável de <see cref="PagedResult{T}"/>.
        /// </summary>
        /// <param name="items">Coleção de itens da página.</param>
        /// <param name="totalCount">Contagem total de registros no banco.</param>
        /// <param name="pageNumber">Número da página atual.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        public PagedResult(IEnumerable<T>? items, long totalCount, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber <= 0 ? 1 : pageNumber;
            PageSize = pageSize <= 0 ? 10 : pageSize;
            TotalCount = totalCount < 0 ? 0 : totalCount;

            Items = items != null
                ? new ReadOnlyCollection<T>(items.ToList())
                : EmptyList;

            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
        }

        /// <summary>
        /// Cria uma nova instância de <see cref="PagedResult{T}"/>.
        /// </summary>
        /// <param name="items">Lista de itens retornados.</param>
        /// <param name="totalCount">Total geral de registros encontrados.</param>
        /// <param name="pageNumber">Número da página atual.</param>
        /// <param name="pageSize">Quantidade de itens por página.</param>
        /// <returns>Uma nova instância imutável de <see cref="PagedResult{T}"/>.</returns>
        public static PagedResult<T> Create(IEnumerable<T>? items, long totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Cria um resultado paginado vazio (sem itens e com contagem zero).
        /// </summary>
        /// <param name="pageNumber">Número da página atual (padrão: 1).</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão: 10).</param>
        /// <returns>Instância de <see cref="PagedResult{T}"/> vazia.</returns>
        public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
        {
            return new PagedResult<T>(EmptyList, 0, pageNumber, pageSize);
        }
    }
}

