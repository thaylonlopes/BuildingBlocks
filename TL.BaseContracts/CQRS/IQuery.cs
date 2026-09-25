namespace TL.BaseContracts.CQRS
{
    /// <summary>
    /// Contrato para consultas (queries) do padrão CQRS que retornam dados sem modificar o estado do sistema.
    /// </summary>
    /// <typeparam name="TResult">O tipo retornado pela consulta.</typeparam>
    public interface IQuery<TResult> : IRequest<TResult>
    {
    }
}

