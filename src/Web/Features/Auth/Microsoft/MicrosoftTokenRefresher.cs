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
    MicrosoftTokenProvider tokenProvider,
    ILogger<MicrosoftTokenRefresher> logger
) : IAuthProviderRefresher
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly MicrosoftTokenProvider _tokenProvider = tokenProvider;
    private readonly ILogger<MicrosoftTokenRefresher> _logger = logger;

    public string ProviderName => MicrosoftEntraAuthModule.PROVIDER_NAME;

    public async Task<bool> RefreshTokensAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            _logger.LogWarning("Cannot refresh Microsoft token: Invalid identity");
            return false;
        }

        var refreshToken = identity.Claims.FirstOrDefault(c => c.Type == MicrosoftTokenProvider.CLAIM_REFRESH_TOKEN)?.Value;
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Cannot refresh Microsoft access token: No refresh token available");
            return false;
        }

        var clientId = _configuration["Authentication:Microsoft:ClientId"];
        var clientSecret = _configuration["Authentication:Microsoft:ClientSecret"];
        var tenantId = _configuration["Authentication:Microsoft:TenantId"] ?? "common";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogError("Cannot refresh Microsoft access token: Missing client configuration");
            return false;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            });

            var response = await httpClient.PostAsync(tokenEndpoint, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to refresh Microsoft access token: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>();

            if (tokenResponse == null)
            {
                _logger.LogWarning("Microsoft token refresh response could not be deserialized");
                return false;
            }

            _tokenProvider.StoreTokens(identity, tokenResponse.AccessToken, tokenResponse.ExpiresIn, tokenResponse.RefreshToken);

            _logger.LogInformation("Microsoft access token refreshed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Microsoft access token");
            return false;
        }
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
