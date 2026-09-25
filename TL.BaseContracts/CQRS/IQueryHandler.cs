using System.Threading;
using System.Threading.Tasks;

namespace TL.BaseContracts.CQRS
{
    /// <summary>
    /// Contrato para o manipulador responsável pela execução assíncrona de uma consulta CQRS.
    /// </summary>
    /// <typeparam name="TQuery">O tipo da consulta a ser executada.</typeparam>
    /// <typeparam name="TResult">O tipo retornado pela execução da consulta.</typeparam>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Manipula a execução assíncrona da consulta informada.
        /// </summary>
        /// <param name="query">A instância da consulta a ser executada.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação cooperativa.</param>
        /// <returns>Uma tarefa representando a operação assíncrona contendo o resultado da consulta.</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}

