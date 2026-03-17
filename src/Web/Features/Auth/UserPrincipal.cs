using Microsoft.AspNetCore.Authentication.JwtBearer;
using Persistence.Auth;
using System.Security.Claims;

namespace Web.Features.Auth;

public class UserPrincipal(ClaimsPrincipal principal) : ICurrentUser
{
    public const string CLAIM_AUTH_PROVIDER = "auth_provider";

    protected readonly ClaimsPrincipal _principal = principal;

    public static UserPrincipal Create()
    {
        var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        return new UserPrincipal(principal);
    }

    public Guid? UserId
    {
        get => Guid.TryParse(_principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        set => SetClaim(ClaimTypes.NameIdentifier, value?.ToString());
    }

    public string? Name
    {
        get => _principal.Identity?.Name;
        set => SetClaim(ClaimTypes.Name, value);
    }

    public string? Email
    {
        get => _principal.FindFirstValue(ClaimTypes.Email);
        set => SetClaim(ClaimTypes.Email, value);
    }

    public string? Provider
    {
        get => _principal.FindFirstValue(CLAIM_AUTH_PROVIDER);
        set => SetClaim(CLAIM_AUTH_PROVIDER, value);
    }

    public bool IsAuthenticated => _principal.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal Principal => _principal;

    protected void SetClaim(string type, string? value)
    {
        if (_principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var existingClaim = identity.FindFirst(type);
        if (existingClaim != null)
        {
            identity.TryRemoveClaim(existingClaim);
        }

        if (!string.IsNullOrEmpty(value))
        {
            identity.AddClaim(new Claim(type, value));
        }
    }
}
