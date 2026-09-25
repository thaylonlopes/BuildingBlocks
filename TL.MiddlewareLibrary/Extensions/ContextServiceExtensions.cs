using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TL.BaseContracts.Context;
using TL.MiddlewareLibrary.Context;

namespace TL.MiddlewareLibrary.Extensions;

/// <summary>
/// Métodos de extensão para registro de serviços de contexto de usuário e multi-tenant no contêiner de injeção de dependência.
/// </summary>
public static class ContextServiceExtensions
{
    /// <summary>
    /// Registra a implementação de <see cref="ICurrentUser"/> baseada em <see cref="IHttpContextAccessor"/>.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <returns>A coleção de serviços para encadeamento fluente.</returns>
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUser, HttpContextCurrentUser>();

        return services;
    }

    /// <summary>
    /// Registra a implementação de <see cref="ICurrentTenant"/> baseada em cabeçalho HTTP configurável ou claims.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="tenantHeaderName">Nome do cabeçalho HTTP a ser inspecionado (padrão: X-Tenant-Id).</param>
    /// <returns>A coleção de serviços para encadeamento fluente.</returns>
    public static IServiceCollection AddCurrentTenant(this IServiceCollection services, string tenantHeaderName = "X-Tenant-Id")
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentTenant>(serviceProvider =>
        {
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            return new HttpContextCurrentTenant(httpContextAccessor, tenantHeaderName);
        });

        return services;
    }
}
