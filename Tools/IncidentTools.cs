using McpHttpServer.Attributes;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

namespace ServiceNowMcp.Tools;

/// <summary>
/// MCP Tool handlers for ServiceNow Incident operations.
/// </summary>
[McpHandler]
public class IncidentTools
{
    [McpTool("get_incident", "Get a ServiceNow incident by sys_id or INC number")]
    public static string GetIncident(
        [McpParameter("The incident sys_id (32-char hex) or INC number (e.g., INC0010001)")]
        string identifier)
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IIncidentReader>();
            return reader.GetByIdAsync(identifier).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error getting incident: {ex.Message}";
        }
    }

    [McpTool("query_incidents", "Query ServiceNow incidents with filters")]
    public static string QueryIncidents(
        [McpParameter("ServiceNow encoded query (e.g., active=true^priority=1)")]
        string query = "",
        [McpParameter("Maximum results to return (default: 10)")]
        int limit = 10,
        [McpParameter("Pagination offset (default: 0)")]
        int offset = 0,
        [McpParameter("Comma-separated field names to return")]
        string fields = "")
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IIncidentReader>();
            return reader.QueryAsync(
                NullIfEmpty(query), limit, offset, NullIfEmpty(fields)
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error querying incidents: {ex.Message}";
        }
    }

    [McpTool("create_incident", "Create a new ServiceNow incident")]
    public static string CreateIncident(
        [McpParameter("Short description of the incident")]
        string short_description,
        [McpParameter("Urgency: 1=High, 2=Medium, 3=Low")]
        string urgency = "3",
        [McpParameter("Impact: 1=High, 2=Medium, 3=Low")]
        string impact = "3",
        [McpParameter("Category (e.g., network, hardware, software)")]
        string category = "",
        [McpParameter("Caller sys_id or display name")]
        string caller_id = "",
        [McpParameter("Assignment group sys_id or name")]
        string assignment_group = "",
        [McpParameter("CI name or sys_id")]
        string cmdb_ci = "",
        [McpParameter("Detailed description")]
        string description = "")
    {
        try
        {
            var writer = ServiceLocator.GetRequired<IIncidentWriter>();
            var resolver = ServiceLocator.GetRequired<ReferenceFieldResolver>();

            var data = BuildIncidentPayload(
                resolver, short_description, urgency, impact,
                category, caller_id, assignment_group,
                cmdb_ci, description);

            return writer.CreateAsync(data).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error creating incident: {ex.Message}";
        }
    }

    [McpTool("update_incident", "Update an existing ServiceNow incident")]
    public static string UpdateIncident(
        [McpParameter("Incident sys_id to update")]
        string sys_id,
        [McpParameter("New short description")]
        string short_description = "",
        [McpParameter("New state (1=New, 2=InProgress, 6=Resolved, 7=Closed)")]
        string state = "",
        [McpParameter("Assigned to user sys_id or name")]
        string assigned_to = "",
        [McpParameter("Close code (for resolution)")]
        string close_code = "",
        [McpParameter("Close notes (for resolution)")]
        string close_notes = "")
    {
        try
        {
            var writer = ServiceLocator.GetRequired<IIncidentWriter>();
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
            return $"Error updating incident: {ex.Message}";
        }
    }

    [McpTool("add_incident_comment", "Add a work note or comment to an incident")]
    public static string AddIncidentComment(
        [McpParameter("Incident sys_id")]
        string sys_id,
        [McpParameter("Comment or work note text")]
        string comment,
        [McpParameter("True for work note, false for customer comment")]
        bool is_work_note = false)
    {
        try
        {
            var writer = ServiceLocator.GetRequired<IIncidentWriter>();
            return writer.AddCommentAsync(sys_id, comment, is_work_note)
                .GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"Error adding comment: {ex.Message}";
        }
    }

    private static Dictionary<string, string> BuildIncidentPayload(
        ReferenceFieldResolver resolver,
        string shortDesc, string urgency, string impact,
        string category, string callerId, string assignmentGroup,
        string cmdbCi, string description)
    {
        var data = new Dictionary<string, string>
        {
            ["short_description"] = shortDesc,
            ["urgency"] = urgency,
            ["impact"] = impact,
        };

        AddIfNotEmpty(data, "category", category);
        AddIfNotEmpty(data, "description", description);
        ResolveAndAdd(data, resolver, "caller_id", "sys_user", "name", callerId);
        ResolveAndAdd(data, resolver, "assignment_group", "sys_user_group", "name", assignmentGroup);
        ResolveAndAdd(data, resolver, "cmdb_ci", "cmdb_ci", "name", cmdbCi);

        return data;
    }

    private static void ResolveAndAdd(
        Dictionary<string, string> data,
        ReferenceFieldResolver resolver,
        string field, string table, string displayField, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        var resolved = resolver.ResolveAsync(table, displayField, value)
            .GetAwaiter().GetResult();
        data[field] = resolved;
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
