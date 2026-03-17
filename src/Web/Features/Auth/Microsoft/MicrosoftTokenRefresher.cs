using Persistence.Auth.Claims;
using Persistence.Auth.Claims.Commands;
using Persistence.Auth.Claims.Queries;
using Persistence.Shared.Cqrs;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Web.Features.Auth;

namespace Web.Features.Auth.Microsoft;

/// <summary>
/// Implements token refresh for Microsoft Entra ID authentication.
/// Automatically refreshes Microsoft access tokens during bearer token refresh.
/// </summary>
public class MicrosoftTokenRefresher(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IServiceProvider serviceProvider,
    ILogger<MicrosoftTokenRefresher> logger
) : IAuthProviderRefresher
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<MicrosoftTokenRefresher> _logger = logger;

    public string ProviderName => MicrosoftEntraAuthModule.PROVIDER_NAME;

    public async Task PersistTokensAsync(UserPrincipal user, CommandExecutor commandExecutor)
    {
        if (user.UserId == null) return;

        var msUser = new MicrosoftUserPrincipal(user.Principal, _serviceProvider);

        if (msUser.AccessToken == null) return;

        var claims = new[]
        {
            (MicrosoftUserPrincipal.CLAIM_ACCESS_TOKEN, msUser.AccessToken),
            (MicrosoftUserPrincipal.CLAIM_REFRESH_TOKEN, msUser.RefreshToken),
            (MicrosoftUserPrincipal.CLAIM_ACCESS_TOKEN_EXPIRES_AT, msUser.AccessTokenExpiresAt?.ToString("o"))
        };

        await commandExecutor.Execute<UserClaimsSetRequest>(
            new UserClaimsSetRequest(user.UserId.Value, MicrosoftEntraAuthModule.PROVIDER_NAME, claims)
        );

        // Strip provider tokens from the principal so they are not embedded in the JWT
        msUser.AccessToken = null;
        msUser.RefreshToken = null;
        msUser.AccessTokenExpiresAt = null;
    }

    public async Task LoadTokensAsync(UserPrincipal user, QueryExecutor queryExecutor)
    {
        if (user.UserId == null) return;

        var claims = await queryExecutor.Execute<UserClaimRequest, IAsyncEnumerable<UserClaim>>(
            new UserClaimRequest(user.UserId.Value, MicrosoftEntraAuthModule.PROVIDER_NAME)
        );

        var msUser = new MicrosoftUserPrincipal(user.Principal, _serviceProvider);

        await foreach (var claim in claims)
        {
            switch (claim.ClaimType)
            {
                case MicrosoftUserPrincipal.CLAIM_ACCESS_TOKEN:
                    msUser.AccessToken = claim.ClaimValue;
                    break;
                case MicrosoftUserPrincipal.CLAIM_REFRESH_TOKEN:
                    msUser.RefreshToken = claim.ClaimValue;
                    break;
                case MicrosoftUserPrincipal.CLAIM_ACCESS_TOKEN_EXPIRES_AT:
                    if (DateTimeOffset.TryParse(claim.ClaimValue, out var expiresAt))
                    {
                        msUser.AccessTokenExpiresAt = expiresAt;
                    }
                    break;
            }
        }
    }

    public async Task RefreshTokensAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity)
        {
            throw new InvalidOperationException("Cannot refresh Microsoft token: Invalid identity");
        }

        var user = new MicrosoftUserPrincipal(principal, _serviceProvider);

        if (string.IsNullOrEmpty(user.RefreshToken))
        {
            throw new InvalidOperationException("Cannot refresh Microsoft access token: No refresh token available");
        }

        var clientId = _configuration["Authentication:Microsoft:ClientId"];
        var clientSecret = _configuration["Authentication:Microsoft:ClientSecret"];
        var tenantId = _configuration["Authentication:Microsoft:TenantId"] ?? "common";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Cannot refresh Microsoft access token: Missing client configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

        var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = user.RefreshToken
        });

        var response = await httpClient.PostAsync(tokenEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to refresh Microsoft access token: {response.StatusCode} - {errorContent}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>()
            ?? throw new InvalidOperationException("Microsoft token refresh response could not be deserialized");

        user.AccessToken = tokenResponse.AccessToken;
        user.AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        // Microsoft returns null if the refresh token doesn't need to be updated
        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
        {
            user.RefreshToken = tokenResponse.RefreshToken;
        }

        _logger.LogInformation("Microsoft access token refreshed successfully");
    }

    private record MicrosoftTokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; init; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public required int ExpiresIn { get; init; }
    }
}
