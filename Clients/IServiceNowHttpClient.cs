namespace ServiceNowMcp.Clients;

/// <summary>
/// Abstraction over HTTP operations against the ServiceNow REST API.
/// All methods return raw JSON response strings.
/// </summary>
public interface IServiceNowHttpClient
{
    /// <summary>Sends a GET request with optional query parameters.</summary>
    Task<string> GetAsync(
        string endpoint,
        Dictionary<string, string>? queryParams = null);

    /// <summary>Sends a POST request with a JSON body.</summary>
    Task<string> PostAsync(string endpoint, object data);

    /// <summary>Sends a PATCH request with a JSON body.</summary>
    Task<string> PatchAsync(string endpoint, object data);

    /// <summary>Sends a PUT request with a JSON body.</summary>
    Task<string> PutAsync(string endpoint, object data);

    /// <summary>Sends a DELETE request.</summary>
    Task<string> DeleteAsync(string endpoint);
}
