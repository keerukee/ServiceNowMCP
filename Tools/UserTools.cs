using McpHttpServer.Attributes;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

namespace ServiceNowMcp.Tools;

/// <summary>
/// MCP Tool handlers for ServiceNow User operations.
/// </summary>
[McpHandler]
public class UserTools
{
    [McpTool("get_user", "Get a ServiceNow user by sys_id or username")]
    public static string GetUser(
        [McpParameter("User sys_id (32-char hex) or user_name")]
        string identifier)
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IUserReader>();

            if (identifier.Length == 32 && identifier.All(char.IsAsciiHexDigit))
                return reader.GetByIdAsync(identifier).GetAwaiter().GetResult();

            return reader.GetByUsernameAsync(identifier).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error getting user: {ex.Message}";
        }
    }

    [McpTool("query_users", "Query ServiceNow users with filters")]
    public static string QueryUsers(
        [McpParameter("ServiceNow encoded query (e.g., active=true^department.name=IT)")]
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
            var reader = ServiceLocator.GetRequired<IUserReader>();
            return reader.QueryAsync(
                NullIfEmpty(query), limit, offset, NullIfEmpty(fields)
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error querying users: {ex.Message}";
        }
    }

    private static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
