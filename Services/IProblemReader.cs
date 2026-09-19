namespace ServiceNowMcp.Services;

/// <summary>
/// Read operations for the ServiceNow problem table.
/// </summary>
public interface IProblemReader
{
    Task<string> GetByIdAsync(string sysIdOrNumber);
    Task<string> QueryAsync(
        string? query = null,
        int limit = 10,
        int offset = 0,
        string? fields = null);
}
