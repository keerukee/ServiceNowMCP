namespace ServiceNowMcp.Services;

/// <summary>
/// Read operations for the ServiceNow sc_request table.
/// </summary>
public interface IServiceCatalogReader
{
    Task<string> GetRequestByIdAsync(string sysIdOrNumber);
    Task<string> QueryRequestsAsync(
        string? query = null,
        int limit = 10,
        int offset = 0,
        string? fields = null);
}
