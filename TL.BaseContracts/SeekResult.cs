using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TL.BaseContracts
{
    /// <summary>
    /// Encapsula o resultado de uma consulta paginada por cursor (Keyset e Seek Method O(1)).
    /// </summary>
    /// <typeparam name="T">O tipo dos elementos retornados na lista.</typeparam>
    /// <typeparam name="TCursor">O tipo da chave ou cursor de continuidade.</typeparam>
    public class SeekResult<T, TCursor>
    {
        private static readonly IReadOnlyList<T> EmptyList = new ReadOnlyCollection<T>(new List<T>());

        /// <summary>
        /// Lista imutável contendo os itens da página atual.
        /// </summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// Quantidade de itens solicitada para a página.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Indica se há uma próxima página disponível para leitura contínua.
        /// </summary>
        public bool HasNextPage { get; }

        /// <summary>
        /// O token ou valor de cursor que identifica o ponto de continuidade para a próxima página.
        /// </summary>
        public TCursor? NextCursor { get; }

        /// <summary>
        /// Inicializa uma nova instância imutável de <see cref="SeekResult{T, TCursor}"/>.
        /// </summary>
        /// <param name="items">Coleção de itens retornados.</param>
        /// <param name="pageSize">Tamanho de página aplicado.</param>
        /// <param name="hasNextPage">Indica se existem mais registros após esta página.</param>
        /// <param name="nextCursor">Valor do cursor para a próxima página.</param>
        public SeekResult(IEnumerable<T>? items, int pageSize, bool hasNextPage, TCursor? nextCursor)
        {
            PageSize = pageSize <= 0 ? 10 : pageSize;
            HasNextPage = hasNextPage;
            NextCursor = nextCursor;

            Items = items != null
                ? new ReadOnlyCollection<T>(items.ToList())
                : EmptyList;
        }

        /// <summary>
        /// Cria uma nova instância de <see cref="SeekResult{T, TCursor}"/>.
        /// </summary>
        /// <param name="items">Lista de itens retornados.</param>
        /// <param name="pageSize">Quantidade de itens solicitada.</param>
        /// <param name="hasNextPage">Indica se há próxima página.</param>
        /// <param name="nextCursor">Identificador de cursor da próxima página.</param>
        /// <returns>Uma nova instância imutável de resultado paginado por cursor.</returns>
        public static SeekResult<T, TCursor> Create(IEnumerable<T>? items, int pageSize, bool hasNextPage, TCursor? nextCursor)
        {
            return new SeekResult<T, TCursor>(items, pageSize, hasNextPage, nextCursor);
        }

        /// <summary>
        /// Cria um resultado paginado por cursor vazio.
        /// </summary>
        /// <param name="pageSize">Tamanho da página (padrão: 10).</param>
        /// <returns>Uma instância vazia de <see cref="SeekResult{T, TCursor}"/>.</returns>
        public static SeekResult<T, TCursor> Empty(int pageSize = 10)
        {
            return new SeekResult<T, TCursor>(EmptyList, pageSize, false, default);
        }
    }

    /// <summary>
    /// Encapsula o resultado de uma consulta paginada por cursor utilizando tokens em formato string.
    /// </summary>
    /// <typeparam name="T">O tipo dos elementos retornados na lista.</typeparam>
    public class SeekResult<T> : SeekResult<T, string>
    {
        /// <summary>
        /// Inicializa uma nova instância com cursor do tipo string.
        /// </summary>
        /// <param name="items">Coleção de itens da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <param name="hasNextPage">Indica se há próxima página.</param>
        /// <param name="nextCursor">Token opaco em formato string para a próxima página.</param>
        public SeekResult(IEnumerable<T>? items, int pageSize, bool hasNextPage, string? nextCursor)
            : base(items, pageSize, hasNextPage, nextCursor)
        {
        }

        /// <summary>
        /// Cria uma nova instância de <see cref="SeekResult{T}"/>.
        /// </summary>
        public static new SeekResult<T> Create(IEnumerable<T>? items, int pageSize, bool hasNextPage, string? nextCursor)
        {
            return new SeekResult<T>(items, pageSize, hasNextPage, nextCursor);
        }

        /// <summary>
        /// Cria um resultado vazio com cursor de string.
        /// </summary>
        public static new SeekResult<T> Empty(int pageSize = 10)
        {
            return new SeekResult<T>(null, pageSize, false, null);
        }
    }
}
