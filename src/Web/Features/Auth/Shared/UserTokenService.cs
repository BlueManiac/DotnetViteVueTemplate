using Microsoft.IdentityModel.Tokens;
using Persistence.Auth.Claims;
using Persistence.Auth.Tokens.Commands;
using Persistence.Auth.Users.Commands;
using Persistence.Shared.Cqrs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Web.Features.Auth;

public class UserTokenService(
    IConfiguration configuration,
    CommandExecutor commandExecutor,
    UserClaimService claimsLoader,
    IEnumerable<IAuthProviderRefresher> tokenRefreshers)
{
    public record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType = "Bearer");

    public async Task<TokenResponse> IssueTokensAsync(
        UserPrincipal user,
        string? deviceInfo = null)
    {
        var userId = await commandExecutor.Execute<UserUpsertRequest, Guid>(
            new UserUpsertRequest(
                Id: user.UserId,
                Email: user.Email,
                Name: user.Name,
                Provider: user.Provider
            )
        );

        user.UserId = userId;

        // Persist provider tokens to DB before JWT generation
        var refresher = tokenRefreshers.FirstOrDefault(r => r.ProviderName == user.Provider);
        if (refresher != null)
        {
            await refresher.PersistTokensAsync(user, commandExecutor);
            claimsLoader.Invalidate(refresher.ProviderName, userId);
        }

        var expirationMinutes = configuration.GetValue("Authentication:AccessTokenExpirationMinutes", 60);
        var accessToken = GenerateAccessToken(user.Principal, expirationMinutes, refresher);

        var refreshToken = await commandExecutor.Execute<UserTokenCreateRequest, UserTokenCreateResponse>(
            new UserTokenCreateRequest(
                UserId: userId,
                DeviceInfo: deviceInfo
            )
        );

        return new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            ExpiresIn: (int)TimeSpan.FromMinutes(expirationMinutes).TotalSeconds
        );
    }

    private string GenerateAccessToken(ClaimsPrincipal principal, int expirationMinutes, IAuthProviderRefresher? refresher = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var allowedClaimTypes = refresher != null
            ? UserPrincipal.BaseClaimTypes.Union(refresher.PublicClaimTypes)
            : UserPrincipal.BaseClaimTypes;

        var claims = principal.Claims.Where(c => allowedClaimTypes.Contains(c.Type));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
