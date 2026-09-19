using McpHttpServer.Attributes;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

namespace ServiceNowMcp.Tools;

/// <summary>
/// MCP Tool handlers for ServiceNow Problem operations.
/// </summary>
[McpHandler]
public class ProblemTools
{
    [McpTool("get_problem", "Get a ServiceNow problem by sys_id or PRB number")]
    public static string GetProblem(
        [McpParameter("Problem sys_id or PRB number (e.g., PRB0000001)")]
        string identifier)
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IProblemReader>();
            return reader.GetByIdAsync(identifier).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error getting problem: {ex.Message}";
        }
    }

    [McpTool("query_problems", "Query ServiceNow problems with filters")]
    public static string QueryProblems(
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
            var reader = ServiceLocator.GetRequired<IProblemReader>();
            return reader.QueryAsync(
                NullIfEmpty(query), limit, offset, NullIfEmpty(fields)
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error querying problems: {ex.Message}";
        }
    }

    [McpTool("create_problem", "Create a new ServiceNow problem record")]
    public static string CreateProblem(
        [McpParameter("Short description of the problem")]
        string short_description,
        [McpParameter("Urgency: 1=High, 2=Medium, 3=Low")]
        string urgency = "3",
        [McpParameter("Impact: 1=High, 2=Medium, 3=Low")]
        string impact = "3",
        [McpParameter("CI name or sys_id")]
        string cmdb_ci = "",
        [McpParameter("Assignment group name or sys_id")]
        string assignment_group = "",
        [McpParameter("Detailed description")]
        string description = "")
    {
        try
        {
            var writer = ServiceLocator.GetRequired<IProblemWriter>();
            var resolver = ServiceLocator.GetRequired<ReferenceFieldResolver>();

            var data = new Dictionary<string, string>
            {
                ["short_description"] = short_description,
                ["urgency"] = urgency,
                ["impact"] = impact,
            };

            AddIfNotEmpty(data, "description", description);
            ResolveAndAdd(data, resolver, "cmdb_ci", "cmdb_ci", "name", cmdb_ci);
            ResolveAndAdd(data, resolver, "assignment_group", "sys_user_group", "name", assignment_group);

            return writer.CreateAsync(data).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error creating problem: {ex.Message}";
        }
    }

    [McpTool("update_problem", "Update an existing ServiceNow problem")]
    public static string UpdateProblem(
        [McpParameter("Problem sys_id")]
        string sys_id,
        [McpParameter("New state")]
        string state = "",
        [McpParameter("New short description")]
        string short_description = "",
        [McpParameter("Root cause notes")]
        string cause_notes = "",
        [McpParameter("Workaround")]
        string work_around = "",
        [McpParameter("Fix notes")]
        string fix_notes = "")
    {
        try
        {
            var writer = ServiceLocator.GetRequired<IProblemWriter>();
            var data = new Dictionary<string, string>();

            AddIfNotEmpty(data, "state", state);
            AddIfNotEmpty(data, "short_description", short_description);
            AddIfNotEmpty(data, "cause_notes", cause_notes);
            AddIfNotEmpty(data, "work_around", work_around);
            AddIfNotEmpty(data, "fix_notes", fix_notes);

            return writer.UpdateAsync(sys_id, data).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error updating problem: {ex.Message}";
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
