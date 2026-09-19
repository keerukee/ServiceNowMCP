namespace ServiceNowMcp.Auth;

/// <summary>
/// Provides OAuth 2.0 access tokens for authenticating
/// against the ServiceNow REST API.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Returns a valid Bearer access token, refreshing automatically
    /// when the cached token is near expiration.
    /// </summary>
    Task<string> GetAccessTokenAsync();
}
