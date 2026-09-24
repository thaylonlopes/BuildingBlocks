namespace TL.BaseContracts.CQRS
{
    /// <summary>
    /// Contrato para comandos do padrão CQRS com retorno tipado de resultado.
    /// </summary>
    /// <typeparam name="TResult">O tipo retornado pela execução do comando (geralmente uma especialização de Result).</typeparam>
    public interface ICommand<TResult> : IRequest<TResult>
    {
    }

    /// <summary>
    /// Contrato para comandos do padrão CQRS que retornam uma instância padrão de <see cref="Result"/>.
    /// </summary>
    public interface ICommand : ICommand<Result>
    {
    }
}
