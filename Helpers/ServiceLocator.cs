namespace ServiceNowMcp.Helpers;

/// <summary>
/// Static service locator initialized once from the DI container.
/// Required because McpHttpServer invokes handler tool methods
/// via static reflection, precluding constructor injection.
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _provider;

    /// <summary>
    /// Initializes the locator with the built service provider.
    /// Must be called once during application startup.
    /// </summary>
    public static void Initialize(IServiceProvider provider)
    {
        _provider = provider
            ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// Resolves a service from the DI container.
    /// </summary>
    public static T GetRequired<T>() where T : notnull
    {
        if (_provider is null)
        {
            throw new InvalidOperationException(
                "ServiceLocator has not been initialized.");
        }

        return _provider.GetRequiredService<T>();
    }
}
