using System;

namespace TL.BaseContracts.Domain
{
    /// <summary>
    /// Contrato para entidades que suportam exclusão lógica sem remoção física do banco de dados.
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// Indica se o registro foi marcado como excluído logicamente.
        /// </summary>
        bool IsDeleted { get; }

        /// <summary>
        /// Data e hora em formato UTC em que a exclusão lógica ocorreu.
        /// </summary>
        DateTime? DeletedAtUtc { get; }
    }
}

