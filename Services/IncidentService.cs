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

    public async Task<string> SearchSimilarAsync(
        string queryText,
        bool onlyResolved = true,
        string? category = null,
        string? cmdbCi = null,
        int limit = 5)
    {
        const string fields = "number,short_description,description,state,close_code,close_notes,category,cmdb_ci,resolved_at,closed_at,sys_created_on";
        var baseFilter = BuildSimilarBaseFilter(onlyResolved, category, cmdbCi);
        var sanitized = queryText.Replace("^", " ").Trim();

        // Try Zing full-text search first (indexed across all text fields)
        var fullTextQuery = $"123TEXTQUERY321={sanitized}{baseFilter}^ORDERBYDESCsys_created_on";
        var result = await QueryAsync(fullTextQuery, limit, 0, fields);

        if (HasResults(result))
            return result;

        // Fallback: substring matching across short_description, description, close_notes
        var likeQuery = $"(short_descriptionLIKE{sanitized}^ORdescriptionLIKE{sanitized}^ORclose_notesLIKE{sanitized}){baseFilter}^ORDERBYDESCsys_created_on";
        return await QueryAsync(likeQuery, limit, 0, fields);
    }

    private static string BuildSimilarBaseFilter(bool onlyResolved, string? category, string? cmdbCi)
    {
        var filter = string.Empty;
        if (onlyResolved) filter += "^stateIN6,7";
        if (!string.IsNullOrWhiteSpace(category)) filter += $"^category={category}";
        if (!string.IsNullOrWhiteSpace(cmdbCi)) filter += $"^cmdb_ci={cmdbCi}";
        return filter;
    }

    private static bool HasResults(string json)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("result", out var res) && res.ValueKind == System.Text.Json.JsonValueKind.Array)
                return res.GetArrayLength() > 0;
        }
        catch { }
        return false;
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
