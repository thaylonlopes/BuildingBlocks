using System.Collections.Generic;

namespace TL.BaseContracts.Context;

/// <summary>
/// Abstração para acesso aos dados do usuário autenticado no contexto da execução atual.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Identificador único do usuário. Retorna nulo quando anônimo.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Endereço de e-mail do usuário no contexto.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Lista de papéis ou permissões associadas ao usuário.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Indica se o usuário está autenticado no contexto atual.
    /// </summary>
    bool IsAuthenticated { get; }
}
