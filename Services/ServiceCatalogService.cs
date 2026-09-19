using ServiceNowMcp.Clients;

namespace ServiceNowMcp.Services;

/// <summary>
/// Service implementation for sc_request table read operations.
/// </summary>
public sealed class ServiceCatalogService : IServiceCatalogReader
{
    private readonly IServiceNowHttpClient _client;
    private const string TableName = "sc_request";
    private const string ApiBase = "api/now/table/" + TableName;

    public ServiceCatalogService(IServiceNowHttpClient client)
    {
        _client = client;
    }

    public async Task<string> GetRequestByIdAsync(string sysIdOrNumber)
    {
        if (sysIdOrNumber.StartsWith("REQ", StringComparison.OrdinalIgnoreCase))
        {
            var qp = BuildDisplayParams();
            qp["sysparm_query"] = $"number={sysIdOrNumber}";
            qp["sysparm_limit"] = "1";
            return await _client.GetAsync(ApiBase, qp);
        }

        var queryParams = BuildDisplayParams();
        return await _client.GetAsync($"{ApiBase}/{sysIdOrNumber}", queryParams);
    }

    public async Task<string> QueryRequestsAsync(
        string? query = null,
        int limit = 10,
        int offset = 0,
        string? fields = null)
    {
        var queryParams = BuildDisplayParams();
        queryParams["sysparm_limit"] = limit.ToString();
        queryParams["sysparm_offset"] = offset.ToString();

        if (!string.IsNullOrWhiteSpace(query))
            queryParams["sysparm_query"] = query;

        if (!string.IsNullOrWhiteSpace(fields))
            queryParams["sysparm_fields"] = fields;

        return await _client.GetAsync(ApiBase, queryParams);
    }

    private static Dictionary<string, string> BuildDisplayParams()
    {
        return new Dictionary<string, string>
        {
            ["sysparm_display_value"] = "all",
            ["sysparm_exclude_reference_link"] = "true"
        };
    }
}
