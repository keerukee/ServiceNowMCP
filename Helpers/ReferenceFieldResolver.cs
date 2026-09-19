using System.Text.Json;
using System.Text.Json.Nodes;
using ServiceNowMcp.Clients;

namespace ServiceNowMcp.Helpers;

/// <summary>
/// Resolves human-readable display names to ServiceNow sys_id
/// values by querying the target table. Essential for reference
/// fields like cmdb_ci, assigned_to, and caller_id.
/// </summary>
public sealed class ReferenceFieldResolver
{
    private readonly IServiceNowHttpClient _client;

    public ReferenceFieldResolver(IServiceNowHttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Resolves a display name to a sys_id by querying the specified table.
    /// Returns the original value if it already looks like a sys_id.
    /// </summary>
    public async Task<string> ResolveAsync(
        string tableName,
        string displayField,
        string displayValue)
    {
        if (LooksLikeSysId(displayValue))
            return displayValue;

        var query = $"{displayField}={displayValue}";
        var queryParams = new Dictionary<string, string>
        {
            ["sysparm_query"] = query,
            ["sysparm_fields"] = "sys_id",
            ["sysparm_limit"] = "1"
        };

        var response = await _client.GetAsync(
            $"api/now/table/{tableName}", queryParams);

        return ExtractSysId(response, displayValue);
    }

    /// <summary>
    /// Checks if the value matches the 32-char hex sys_id pattern.
    /// </summary>
    private static bool LooksLikeSysId(string value)
    {
        return value.Length == 32
               && value.All(c => char.IsAsciiHexDigit(c));
    }

    private static string ExtractSysId(string json, string fallback)
    {
        var node = JsonNode.Parse(json);
        var results = node?["result"]?.AsArray();

        if (results is null || results.Count == 0)
            return fallback;

        return results[0]?["sys_id"]?.GetValue<string>() ?? fallback;
    }
}
