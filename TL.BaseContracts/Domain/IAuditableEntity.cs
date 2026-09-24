using System;

namespace TL.BaseContracts.Domain
{
    /// <summary>
    /// Contrato para entidades que exigem rastreamento de autoria e timestamps de criação e atualização em UTC.
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Data e hora de criação do registro em formato UTC.
        /// </summary>
        DateTime CreatedAtUtc { get; }

        /// <summary>
        /// Identificador ou nome do usuário ou serviço responsável pela criação do registro.
        /// </summary>
        string? CreatedBy { get; }

        /// <summary>
        /// Data e hora da última modificação do registro em formato UTC.
        /// </summary>
        DateTime? UpdatedAtUtc { get; }

        /// <summary>
        /// Identificador ou nome do usuário ou serviço responsável pela última modificação do registro.
        /// </summary>
        string? UpdatedBy { get; }
    }
}
