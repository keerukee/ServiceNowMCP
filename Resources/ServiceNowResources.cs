using McpHttpServer.Attributes;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

namespace ServiceNowMcp.Resources;

/// <summary>
/// MCP Resources exposing read-only ServiceNow data via URI-based access.
/// </summary>
[McpHandler]
public class ServiceNowResources
{
    [McpResource(
        "servicenow://reports/active_incidents",
        "Active Incidents Report",
        "Summary of currently active (non-resolved) incidents",
        "application/json")]
    public static string GetActiveIncidents()
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IIncidentReader>();
            return reader.QueryAsync(
                query: "active=true^ORDERBYDESCsys_created_on",
                limit: 25,
                fields: "number,short_description,priority,state,assigned_to,cmdb_ci,sys_created_on"
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
    }

    [McpResource(
        "servicenow://reports/open_changes",
        "Open Change Requests Report",
        "Summary of open (non-closed) change requests",
        "application/json")]
    public static string GetOpenChanges()
    {
        try
        {
            var reader = ServiceLocator.GetRequired<IChangeRequestReader>();
            return reader.QueryAsync(
                query: "state!=3^state!=4^ORDERBYDESCsys_created_on",
                limit: 25,
                fields: "number,short_description,type,risk,state,assigned_to,cmdb_ci"
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
    }

    [McpResource(
        "servicenow://schema/tables",
        "Available Tables Schema",
        "Common ServiceNow tables and their key fields",
        "application/json")]
    public static string GetTablesSchema()
    {
        return """
        {
          "tables": [
            {
              "name": "incident",
              "label": "Incident",
              "key_fields": ["number","short_description","caller_id","cmdb_ci","priority","urgency","impact","state","assigned_to","assignment_group"]
            },
            {
              "name": "change_request",
              "label": "Change Request",
              "key_fields": ["number","short_description","type","risk","impact","state","cmdb_ci","assigned_to","start_date","end_date"]
            },
            {
              "name": "problem",
              "label": "Problem",
              "key_fields": ["number","short_description","priority","state","cmdb_ci","assigned_to","cause_notes","work_around"]
            },
            {
              "name": "cmdb_ci",
              "label": "Configuration Item",
              "key_fields": ["name","sys_class_name","operational_status","ip_address","serial_number","assigned_to"]
            },
            {
              "name": "sys_user",
              "label": "User",
              "key_fields": ["user_name","name","email","active","title","department","manager"]
            },
            {
              "name": "sc_request",
              "label": "Service Catalog Request",
              "key_fields": ["number","requested_for","request_state","stage","special_instructions"]
            }
          ]
        }
        """;
    }
}
