namespace ServiceNowMcp.Configuration;

/// <summary>
/// Reads and validates ServiceNow connection parameters from environment
/// variables or appsettings.json configuration.
/// </summary>
public sealed class ServiceNowConfiguration
{
    /// <summary>
    /// ServiceNow instance base URL (e.g., https://dev12345.service-now.com)
    /// </summary>
    public string InstanceUrl { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 Client ID for Client Credentials flow.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 Client Secret for Client Credentials flow.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Optional username for Resource Owner Password grant fallback.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Optional password for Resource Owner Password grant fallback.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Loads configuration from environment variables, falling back to
    /// the provided IConfiguration section values.
    /// </summary>
    public static ServiceNowConfiguration Load(IConfiguration config)
    {
        var section = config.GetSection("ServiceNow");

        return new ServiceNowConfiguration
        {
            InstanceUrl = ResolveValue("SERVICENOW_INSTANCE_URL", section["InstanceUrl"]),
            ClientId = ResolveValue("SERVICENOW_CLIENT_ID", section["ClientId"]),
            ClientSecret = ResolveValue("SERVICENOW_CLIENT_SECRET", section["ClientSecret"]),
            Username = ResolveValue("SERVICENOW_USERNAME", section["Username"]),
            Password = ResolveValue("SERVICENOW_PASSWORD", section["Password"]),
        };
    }

    /// <summary>
    /// Validates that the minimum required configuration is present.
    /// </summary>
    public (bool IsValid, string Error) Validate()
    {
        if (string.IsNullOrWhiteSpace(InstanceUrl))
            return (false, "SERVICENOW_INSTANCE_URL is not configured.");

        if (string.IsNullOrWhiteSpace(ClientId))
            return (false, "SERVICENOW_CLIENT_ID is not configured.");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            return (false, "SERVICENOW_CLIENT_SECRET is not configured.");

        return (true, string.Empty);
    }

    private static string ResolveValue(string envVar, string? fallback)
    {
        return Environment.GetEnvironmentVariable(envVar)
               ?? fallback
               ?? string.Empty;
    }
}
