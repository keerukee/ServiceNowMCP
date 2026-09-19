namespace ServiceNowMcp.Services;

/// <summary>
/// Read operations for the ServiceNow incident table.
/// </summary>
public interface IIncidentReader
{
    /// <summary>Retrieves an incident by sys_id or number.</summary>
    Task<string> GetByIdAsync(string sysIdOrNumber);

    /// <summary>Queries incidents with an encoded query string.</summary>
    Task<string> QueryAsync(
        string? query = null,
        int limit = 10,
        int offset = 0,
        string? fields = null);

    /// <summary>Retrieves an incident by its INCxxxxxx number.</summary>
    Task<string> GetByNumberAsync(string number);
}
