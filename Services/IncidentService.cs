using ServiceNowMcp.Clients;

namespace ServiceNowMcp.Services;

/// <summary>
/// Service implementation for incident table CRUD operations.
/// </summary>
public sealed class IncidentService : IIncidentReader, IIncidentWriter
{
    private readonly IServiceNowHttpClient _client;
    private const string TableName = "incident";
    private const string ApiBase = "api/now/table/" + TableName;

    public IncidentService(IServiceNowHttpClient client)
    {
        _client = client;
    }

    public async Task<string> GetByIdAsync(string sysIdOrNumber)
    {
        if (sysIdOrNumber.StartsWith("INC", StringComparison.OrdinalIgnoreCase))
            return await GetByNumberAsync(sysIdOrNumber);

        var queryParams = BuildDisplayParams();
        return await _client.GetAsync($"{ApiBase}/{sysIdOrNumber}", queryParams);
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

    public async Task<string> GetByNumberAsync(string number)
    {
        var queryParams = BuildDisplayParams();
        queryParams["sysparm_query"] = $"number={number}";
        queryParams["sysparm_limit"] = "1";
        return await _client.GetAsync(ApiBase, queryParams);
    }

    public async Task<string> CreateAsync(object incidentData)
    {
        return await _client.PostAsync(
            $"{ApiBase}?sysparm_display_value=all", incidentData);
    }

    public async Task<string> UpdateAsync(string sysId, object updateData)
    {
        return await _client.PatchAsync(
            $"{ApiBase}/{sysId}?sysparm_display_value=all", updateData);
    }

    public async Task<string> AddCommentAsync(
        string sysId,
        string comment,
        bool isWorkNote = false)
    {
        var field = isWorkNote ? "work_notes" : "comments";
        var data = new Dictionary<string, string> { [field] = comment };
        return await _client.PatchAsync($"{ApiBase}/{sysId}", data);
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
