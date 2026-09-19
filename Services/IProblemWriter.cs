namespace ServiceNowMcp.Services;

/// <summary>
/// Write operations for the ServiceNow problem table.
/// </summary>
public interface IProblemWriter
{
    Task<string> CreateAsync(object problemData);
    Task<string> UpdateAsync(string sysId, object updateData);
}
