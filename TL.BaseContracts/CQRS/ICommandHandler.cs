using System.Threading;
using System.Threading.Tasks;

namespace TL.BaseContracts.CQRS
{
    /// <summary>
    /// Contrato para o manipulador responsável pela execução assíncrona de um comando CQRS com retorno tipado.
    /// </summary>
    /// <typeparam name="TCommand">O tipo do comando a ser manipulado.</typeparam>
    /// <typeparam name="TResult">O tipo retornado pela execução do comando.</typeparam>
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Manipula a execução assíncrona do comando informado.
        /// </summary>
        /// <param name="command">A instância do comando a ser processado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação cooperativa.</param>
        /// <returns>Uma tarefa representando a operação assíncrona contendo o resultado tipado.</returns>
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Contrato para o manipulador responsável pela execução assíncrona de um comando CQRS que retorna <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="TCommand">O tipo do comando a ser manipulado.</typeparam>
    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Result> where TCommand : ICommand
    {
    }
}

