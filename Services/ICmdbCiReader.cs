namespace ServiceNowMcp.Services;

/// <summary>
/// Read operations for the ServiceNow cmdb_ci table.
/// </summary>
public interface ICmdbCiReader
{
    Task<string> GetByIdAsync(string sysId);
    Task<string> QueryAsync(
        string? query = null,
        int limit = 10,
        int offset = 0,
        string? fields = null);
    Task<string> GetByNameAsync(string name);
}
