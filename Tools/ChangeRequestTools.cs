using McpHttpServer.Attributes;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

namespace ServiceNowMcp.Tools;

/// <summary>
/// MCP Tool handlers for ServiceNow Change Request operations.
/// </summary>
[McpHandler]
public class ChangeRequestTools
{
    [McpTool("get_change_request", "Get a ServiceNow change request by sys_id or CHG number")]
    public static string GetChangeRequest(
        [McpParameter("The change request sys_id or CHG number (e.g., CHG0000001)")]
        string identifier)
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IChangeRequestReader>();
            return reader.GetByIdAsync(identifier).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error getting change request: {ex.Message}";
        }
    }

    [McpTool("query_change_requests", "Query ServiceNow change requests with filters")]
    public static string QueryChangeRequests(
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
            var reader = ServiceLocator.GetRequired<IChangeRequestReader>();
            return reader.QueryAsync(
                NullIfEmpty(query), limit, offset, NullIfEmpty(fields)
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error querying change requests: {ex.Message}";
        }
    }

    [McpTool("create_change_request", "Create a new ServiceNow change request")]
    public static string CreateChangeRequest(
        [McpParameter("Short description")]
        string short_description,
        [McpParameter("Type: normal, standard, or emergency")]
        string type = "normal",
        [McpParameter("Risk: 1=Very High, 2=High, 3=Moderate, 4=Low")]
        string risk = "3",
        [McpParameter("Impact: 1=High, 2=Medium, 3=Low")]
        string impact = "3",
        [McpParameter("CI name or sys_id")]
        string cmdb_ci = "",
        [McpParameter("Assignment group name or sys_id")]
        string assignment_group = "",
        [McpParameter("Implementation plan")]
        string implementation_plan = "",
        [McpParameter("Backout plan")]
        string backout_plan = "",
        [McpParameter("Test plan")]
        string test_plan = "")
    {
        try
        {
            var writer = ServiceLocator.GetRequired<IChangeRequestWriter>();
            var resolver = ServiceLocator.GetRequired<ReferenceFieldResolver>();

            var data = new Dictionary<string, string>
            {
                ["short_description"] = short_description,
                ["type"] = type,
                ["risk"] = risk,
                ["impact"] = impact,
            };

            AddIfNotEmpty(data, "implementation_plan", implementation_plan);
            AddIfNotEmpty(data, "backout_plan", backout_plan);
            AddIfNotEmpty(data, "test_plan", test_plan);
            ResolveAndAdd(data, resolver, "cmdb_ci", "cmdb_ci", "name", cmdb_ci);
            ResolveAndAdd(data, resolver, "assignment_group", "sys_user_group", "name", assignment_group);

            return writer.CreateAsync(data).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error creating change request: {ex.Message}";
        }
    }

    [McpTool("update_change_request", "Update an existing ServiceNow change request")]
    public static string UpdateChangeRequest(
        [McpParameter("Change request sys_id")]
        string sys_id,
        [McpParameter("New state")]
        string state = "",
        [McpParameter("New short description")]
        string short_description = "",
        [McpParameter("Assigned to user sys_id or name")]
        string assigned_to = "",
        [McpParameter("Close code")]
        string close_code = "",
        [McpParameter("Close notes")]
        string close_notes = "")
    {
        try
        {
            var writer = ServiceLocator.GetRequired<IChangeRequestWriter>();
            var data = new Dictionary<string, string>();

            AddIfNotEmpty(data, "short_description", short_description);
            AddIfNotEmpty(data, "state", state);
            AddIfNotEmpty(data, "assigned_to", assigned_to);
            AddIfNotEmpty(data, "close_code", close_code);
            AddIfNotEmpty(data, "close_notes", close_notes);

            return writer.UpdateAsync(sys_id, data).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error updating change request: {ex.Message}";
        }
    }

    private static void ResolveAndAdd(
        Dictionary<string, string> data,
        ReferenceFieldResolver resolver,
        string field, string table, string displayField, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        data[field] = resolver.ResolveAsync(table, displayField, value)
            .GetAwaiter().GetResult();
    }

    private static void AddIfNotEmpty(
        Dictionary<string, string> data, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            data[key] = value;
    }

    private static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
