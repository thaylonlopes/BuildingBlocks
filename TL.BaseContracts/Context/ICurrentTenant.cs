using System;

namespace TL.BaseContracts.Context;

/// <summary>
/// Abstração para isolamento de dados por cliente ou organização em arquiteturas multi-tenant.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// Identificador do tenant ativo na operação atual. Retorna nulo quando não aplicável.
    /// </summary>
    string? TenantId { get; }

#if NETCOREAPP3_0_OR_GREATER || NET8_0_OR_GREATER
    /// <summary>
    /// Indica se existe um tenant válido associado ao contexto corrente.
    /// </summary>
    bool HasTenant => !string.IsNullOrWhiteSpace(TenantId);
#else
    /// <summary>
    /// Indica se existe um tenant válido associado ao contexto corrente.
    /// </summary>
    bool HasTenant { get; }
#endif
}
