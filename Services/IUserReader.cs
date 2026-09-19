namespace ServiceNowMcp.Services;

/// <summary>
/// Read operations for the ServiceNow sys_user table.
/// </summary>
public interface IUserReader
{
    Task<string> GetByIdAsync(string sysId);
    Task<string> QueryAsync(
        string? query = null,
        int limit = 10,
        int offset = 0,
        string? fields = null);
    Task<string> GetByUsernameAsync(string userName);
}
