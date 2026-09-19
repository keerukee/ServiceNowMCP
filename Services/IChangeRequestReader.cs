namespace ServiceNowMcp.Services;

/// <summary>
/// Read operations for the ServiceNow change_request table.
/// </summary>
public interface IChangeRequestReader
{
    Task<string> GetByIdAsync(string sysIdOrNumber);
    Task<string> QueryAsync(
        string? query = null,
        int limit = 10,
        int offset = 0,
        string? fields = null);
    Task<string> GetByNumberAsync(string number);
}
