using McpHttpServer.Attributes;

namespace ServiceNowMcp.Prompts;

/// <summary>
/// MCP Prompts for guided ServiceNow workflows.
/// </summary>
[McpHandler]
public class ServiceNowPrompts
{
    [McpPrompt(
        "servicenow-triage_incident",
        "Guided incident triage workflow with structured questions")]
    public static string TriageIncident(
        [McpArgument("Caller name or username")]
        string caller,
        [McpArgument("Brief summary of the issue")]
        string summary)
    {
        return $"""
            You are a ServiceNow incident triage specialist. A new incident
            has been reported by {caller}.

            Summary: {summary}

            Please follow this triage workflow:
            1. Classify the category (network, hardware, software, database, other).
            2. Determine urgency (1-High, 2-Medium, 3-Low) based on business impact.
            3. Determine impact (1-High, 2-Medium, 3-Low) based on affected users.
            4. Identify the affected Configuration Item (CI) if applicable.
            5. Recommend an assignment group.
            6. Use the create_incident tool to create the incident with the
               determined values.

            Be thorough but concise in your analysis.
            """;
    }

    [McpPrompt(
        "servicenow-create_change",
        "Guided change request creation with all required fields")]
    public static string CreateChangeGuided(
        [McpArgument("What is being changed")]
        string change_summary,
        [McpArgument("Normal, standard, or emergency")]
        string change_type = "normal")
    {
        return $"""
            You are a ServiceNow change management specialist. A change
            request needs to be created.

            Change Summary: {change_summary}
            Change Type: {change_type}

            Please guide through the change creation process:
            1. Draft a clear short_description.
            2. Assess risk level (1-Very High, 2-High, 3-Moderate, 4-Low).
            3. Assess impact level (1-High, 2-Medium, 3-Low).
            4. Identify the affected CI(s).
            5. Write an implementation_plan with step-by-step instructions.
            6. Write a backout_plan for rollback procedures.
            7. Write a test_plan for verification steps.
            8. Recommend an assignment_group.
            9. Use the create_change_request tool to create the change.

            Ensure all plans are detailed and actionable.
            """;
    }

    [McpPrompt(
        "servicenow-analyze_incident_resolution",
        "Analyze a current incident against historical incidents to find identical issues and their resolutions")]
    public static string AnalyzeIncidentResolution(
        [McpArgument("The current incident number (e.g. INC0010001) or description of the current issue")]
        string issue_or_incident_number)
    {
        return $"""
            You are an expert ServiceNow Incident and Problem Resolution Analyst.
            Your task is to analyze the issue described below, locate historical incidents with matching symptoms, and extract proven resolutions.

            Target Issue: {issue_or_incident_number}

            Investigation Workflow:
            1. If an incident number (e.g. INCxxxxxxx) was provided, call `get_incident` to retrieve its full details (short_description, description, category, cmdb_ci).
            2. Extract the core symptoms, error messages, and affected component.
            3. Call `search_similar_incidents` using the extracted symptoms/error text with `only_resolved = true`.
            4. Review the returned historical incidents:
               - Compare the symptoms and root causes.
               - Inspect `close_code` and `close_notes` for workarounds and permanent fixes.
            5. Provide a structured incident resolution report:
               - **Symptom Match Analysis**: How closely past incidents match the current issue.
               - **Identified Root Cause**: Most likely cause based on past patterns.
               - **Recommended Resolution / Workaround**: Step-by-step resolution extracted from past `close_notes`.
               - **Reference Incidents**: List of matching incident numbers and their close codes.
            """;
    }
}
