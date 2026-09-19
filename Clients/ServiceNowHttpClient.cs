using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ServiceNowMcp.Auth;
using ServiceNowMcp.Configuration;

namespace ServiceNowMcp.Clients;

/// <summary>
/// Concrete HTTP client for ServiceNow Table API.
/// Injects ITokenProvider for automatic OAuth Bearer token management.
/// </summary>
public sealed class ServiceNowHttpClient : IServiceNowHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenProvider _tokenProvider;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public ServiceNowHttpClient(
        HttpClient httpClient,
        ITokenProvider tokenProvider,
        ServiceNowConfiguration config)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _baseUrl = config.InstanceUrl.TrimEnd('/');
    }

    public async Task<string> GetAsync(
        string endpoint,
        Dictionary<string, string>? queryParams = null)
    {
        var url = BuildUrl(endpoint, queryParams);
        var request = await CreateRequest(HttpMethod.Get, url);
        return await SendAsync(request);
    }

    public async Task<string> PostAsync(string endpoint, object data)
    {
        var url = BuildUrl(endpoint);
        var request = await CreateRequest(HttpMethod.Post, url);
        request.Content = SerializeBody(data);
        return await SendAsync(request);
    }

    public async Task<string> PatchAsync(string endpoint, object data)
    {
        var url = BuildUrl(endpoint);
        var request = await CreateRequest(HttpMethod.Patch, url);
        request.Content = SerializeBody(data);
        return await SendAsync(request);
    }

    public async Task<string> PutAsync(string endpoint, object data)
    {
        var url = BuildUrl(endpoint);
        var request = await CreateRequest(HttpMethod.Put, url);
        request.Content = SerializeBody(data);
        return await SendAsync(request);
    }

    public async Task<string> DeleteAsync(string endpoint)
    {
        var url = BuildUrl(endpoint);
        var request = await CreateRequest(HttpMethod.Delete, url);
        return await SendAsync(request);
    }

    private string BuildUrl(
        string endpoint,
        Dictionary<string, string>? queryParams = null)
    {
        var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
        if (queryParams is null || queryParams.Count == 0)
            return url;

        var qs = string.Join("&", queryParams
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        return url.Contains('?') ? $"{url}&{qs}" : $"{url}?{qs}";
    }

    private async Task<HttpRequestMessage> CreateRequest(
        HttpMethod method,
        string url)
    {
        var token = await _tokenProvider.GetAccessTokenAsync();
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private static StringContent SerializeBody(object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<string> SendAsync(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"ServiceNow API error ({response.StatusCode}): {content}");
        }

        return string.IsNullOrEmpty(content)
            ? "{\"result\": \"success\"}"
            : content;
    }
}
