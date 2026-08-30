using System;

namespace CommonHelpers.RequestResponse
{
    /// <summary>
    /// Contrato fundamental para identificar e rastrear requisições em microsserviços e pipelines corporativos.
    /// </summary>
    /// <remarks>
    /// Implemente esta interface em DTOs ou Commands para garantir que toda requisição possua um identificador de correlação (<see cref="IdRequest"/>)
    /// único, facilitando a auditoria e o rastreamento ponta a ponta em logs estruturados.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class CreateUserCommand : IRequest
    /// {
    ///     public Guid IdRequest { get; } = Guid.NewGuid();
    ///     public string Username { get; set; } = string.Empty;
    /// }
    /// </code>
    /// </example>
    public interface IRequest
    {
        /// <summary>
        /// Identificador exclusivo da requisição para rastreabilidade e correlação entre serviços.
        /// </summary>
        Guid IdRequest { get; }
    }

    /// <summary>
    /// Contrato genérico para requisições tipadas que transportam um corpo ou payload específico.
    /// </summary>
    /// <typeparam name="T">O tipo do payload ou dados transportados pela requisição.</typeparam>
    public interface IRequest<T> : IRequest
    {
    }
}
