using McpHttpServer.Attributes;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

namespace ServiceNowMcp.Tools;

/// <summary>
/// MCP Tool handlers for ServiceNow CMDB Configuration Item operations.
/// </summary>
[McpHandler]
public class CmdbCiTools
{
    [McpTool("get_cmdb_ci", "Get a CMDB configuration item by sys_id or name")]
    public static string GetCmdbCi(
        [McpParameter("CI sys_id (32-char hex) or display name")]
        string identifier)
    {
        try
        {
            var reader = ServiceLocator.GetRequired<ICmdbCiReader>();

            if (identifier.Length == 32 && identifier.All(char.IsAsciiHexDigit))
                return reader.GetByIdAsync(identifier).GetAwaiter().GetResult();

            return reader.GetByNameAsync(identifier).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error getting CI: {ex.Message}";
        }
    }

    [McpTool("query_cmdb_cis", "Query CMDB configuration items with filters")]
    public static string QueryCmdbCis(
        [McpParameter("ServiceNow encoded query (e.g., operational_status=1)")]
        string query = "",
        [McpParameter("Maximum results (default: 10)")]
        int limit = 10,
        [McpParameter("Pagination offset (default: 0)")]
        int offset = 0,
        [McpParameter("Comma-separated field names")]
        string fields = "")
    {
        try
        {
            var reader = ServiceLocator.GetRequired<ICmdbCiReader>();
            return reader.QueryAsync(
                NullIfEmpty(query), limit, offset, NullIfEmpty(fields)
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error querying CIs: {ex.Message}";
        }
    }

    private static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
