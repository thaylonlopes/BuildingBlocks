using Microsoft.AspNetCore.Http;
using TL.BaseContracts.Context;

namespace TL.MiddlewareLibrary.Context;

/// <summary>
/// Implementação de <see cref="ICurrentTenant"/> que extrai o identificador do tenant a partir de cabeçalhos HTTP ou claims.
/// </summary>
public sealed class HttpContextCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _tenantHeaderName;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="HttpContextCurrentTenant"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Acesso ao HttpContext da requisição corrente.</param>
    /// <param name="tenantHeaderName">Nome do cabeçalho HTTP utilizado para transmitir o tenant (padrão: X-Tenant-Id).</param>
    public HttpContextCurrentTenant(IHttpContextAccessor httpContextAccessor, string tenantHeaderName = "X-Tenant-Id")
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantHeaderName = tenantHeaderName;
    }

    /// <inheritdoc/>
    public string? TenantId => ExtractTenantId();

    private string? ExtractTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Request.Headers.TryGetValue(_tenantHeaderName, out var headerValue))
        {
            var headerString = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(headerString))
            {
                return headerString;
            }
        }

        var principal = httpContext.User;
        var claimValue = principal?.FindFirst("tenant_id")?.Value
            ?? principal?.FindFirst("tenant")?.Value;

        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            return claimValue;
        }

        return null;
    }
}
