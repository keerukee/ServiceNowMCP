using McpHttpServer.Attributes;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

namespace ServiceNowMcp.Tools;

/// <summary>
/// MCP Tool handlers for ServiceNow Service Catalog operations.
/// </summary>
[McpHandler]
public class ServiceCatalogTools
{
    [McpTool("get_sc_request", "Get a service catalog request by sys_id or REQ number")]
    public static string GetScRequest(
        [McpParameter("Request sys_id or REQ number (e.g., REQ0010001)")]
        string identifier)
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IServiceCatalogReader>();
            return reader.GetRequestByIdAsync(identifier)
                .GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error getting request: {ex.Message}";
        }
    }

    [McpTool("query_sc_requests", "Query service catalog requests with filters")]
    public static string QueryScRequests(
        [McpParameter("ServiceNow encoded query")]
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
            var reader = ServiceLocator.GetRequired<IServiceCatalogReader>();
            return reader.QueryRequestsAsync(
                NullIfEmpty(query), limit, offset, NullIfEmpty(fields)
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error querying requests: {ex.Message}";
        }
    }

    private static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
