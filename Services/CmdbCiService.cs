using ServiceNowMcp.Clients;

namespace ServiceNowMcp.Services;

/// <summary>
/// Service implementation for cmdb_ci table read operations.
/// </summary>
public sealed class CmdbCiService : ICmdbCiReader
{
    private readonly IServiceNowHttpClient _client;
    private const string TableName = "cmdb_ci";
    private const string ApiBase = "api/now/table/" + TableName;

    public CmdbCiService(IServiceNowHttpClient client)
    {
        _client = client;
    }

    public async Task<string> GetByIdAsync(string sysId)
    {
        var queryParams = BuildDisplayParams();
        return await _client.GetAsync($"{ApiBase}/{sysId}", queryParams);
    }

    public async Task<string> QueryAsync(
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

    public async Task<string> GetByNameAsync(string name)
    {
        var queryParams = BuildDisplayParams();
        queryParams["sysparm_query"] = $"name={name}";
        queryParams["sysparm_limit"] = "1";
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
