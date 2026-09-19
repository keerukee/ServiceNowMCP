using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ServiceNowMcp.Configuration;

namespace ServiceNowMcp.Auth;

/// <summary>
/// Acquires OAuth 2.0 tokens via the Client Credentials grant
/// against the ServiceNow /oauth_token.do endpoint.
/// Caches the token and auto-refreshes before expiration.
/// </summary>
public sealed class OAuthClientCredentialsProvider : ITokenProvider
{
    private readonly ServiceNowConfiguration _config;
    private readonly HttpClient _httpClient;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private const int ExpiryBufferSeconds = 60;

    public OAuthClientCredentialsProvider(
        ServiceNowConfiguration config,
        HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (IsTokenValid())
            return _cachedToken!;

        await _semaphore.WaitAsync();
        try
        {
            if (IsTokenValid())
                return _cachedToken!;

            await RequestNewTokenAsync();
            return _cachedToken!;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool IsTokenValid()
    {
        return _cachedToken is not null
               && DateTime.UtcNow < _tokenExpiry;
    }

    private async Task RequestNewTokenAsync()
    {
        var tokenUrl = BuildTokenUrl();
        var request = BuildTokenRequest(tokenUrl);
        var response = await _httpClient.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OAuth token request failed ({response.StatusCode}): {body}");
        }

        var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(body);
        _cachedToken = tokenResponse?.AccessToken
            ?? throw new InvalidOperationException("No access_token in response.");

        var expiresIn = tokenResponse.ExpiresIn > 0
            ? tokenResponse.ExpiresIn
            : 1800;

        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - ExpiryBufferSeconds);
    }

    private string BuildTokenUrl()
    {
        return $"{_config.InstanceUrl.TrimEnd('/')}/oauth_token.do";
    }

    private HttpRequestMessage BuildTokenRequest(string tokenUrl)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials"
                })
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        return request;
    }

    private sealed class OAuthTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
