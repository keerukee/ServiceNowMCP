namespace ServiceNowMcp.Services;

/// <summary>
/// Write operations for the ServiceNow change_request table.
/// </summary>
public interface IChangeRequestWriter
{
    Task<string> CreateAsync(object changeData);
    Task<string> UpdateAsync(string sysId, object updateData);
}
