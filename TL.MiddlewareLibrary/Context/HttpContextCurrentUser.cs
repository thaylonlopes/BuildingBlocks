using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TL.BaseContracts.Context;

namespace TL.MiddlewareLibrary.Context;

/// <summary>
/// Implementação de <see cref="ICurrentUser"/> que extrai a identidade e papéis a partir de claims do HttpContext atual.
/// </summary>
public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="HttpContextCurrentUser"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Acesso ao HttpContext da requisição corrente.</param>
    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public string? Id => ExtractUserId();

    /// <inheritdoc/>
    public string? Email => ExtractUserEmail();

    /// <inheritdoc/>
    public IReadOnlyList<string> Roles => ExtractUserRoles();

    /// <inheritdoc/>
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private string? ExtractUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return null;
        }

        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value
            ?? principal.FindFirst("uid")?.Value;
    }

    private string? ExtractUserEmail()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return null;
        }

        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
    }

    private IReadOnlyList<string> ExtractUserRoles()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return [];
        }

        return principal.FindAll(ClaimTypes.Role)
            .Concat(principal.FindAll("role"))
            .Select(claim => claim.Value)
            .Distinct()
            .ToList();
    }
}
