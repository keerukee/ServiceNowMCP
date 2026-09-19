namespace ServiceNowMcp.Services;

/// <summary>
/// Write operations for the ServiceNow incident table.
/// </summary>
public interface IIncidentWriter
{
    /// <summary>Creates a new incident.</summary>
    Task<string> CreateAsync(object incidentData);

    /// <summary>Updates an existing incident by sys_id.</summary>
    Task<string> UpdateAsync(string sysId, object updateData);

    /// <summary>Adds a work note or comment to an incident.</summary>
    Task<string> AddCommentAsync(
        string sysId,
        string comment,
        bool isWorkNote = false);
}
