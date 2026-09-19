using McpHttpServer;
using ServiceNowMcp.Auth;
using ServiceNowMcp.Clients;
using ServiceNowMcp.Configuration;
using ServiceNowMcp.Helpers;
using ServiceNowMcp.Services;

// ─── Load Configuration ───────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

var snConfig = ServiceNowConfiguration.Load(builder.Configuration);
var (isValid, error) = snConfig.Validate();
if (!isValid)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[CONFIG ERROR] {error}");
    Console.ResetColor();
}

// ─── Register Core Services ───────────────────────────────────────
builder.Services.AddSingleton(snConfig);
builder.Services.AddHttpClient();

// Auth: OAuth 2.0 Client Credentials token provider
builder.Services.AddSingleton<ITokenProvider>(sp =>
{
    var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
    return new OAuthClientCredentialsProvider(
        snConfig, httpFactory.CreateClient("ServiceNowOAuth"));
});

// HTTP client abstraction
builder.Services.AddSingleton<IServiceNowHttpClient>(sp =>
{
    var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
    var tokenProvider = sp.GetRequiredService<ITokenProvider>();
    return new ServiceNowHttpClient(
        httpFactory.CreateClient("ServiceNowApi"),
        tokenProvider, snConfig);
});

// ─── Register Domain Services (ISP Interfaces) ───────────────────
builder.Services.AddSingleton<IncidentService>();
builder.Services.AddSingleton<IIncidentReader>(sp => sp.GetRequiredService<IncidentService>());
builder.Services.AddSingleton<IIncidentWriter>(sp => sp.GetRequiredService<IncidentService>());

builder.Services.AddSingleton<ChangeRequestService>();
builder.Services.AddSingleton<IChangeRequestReader>(sp => sp.GetRequiredService<ChangeRequestService>());
builder.Services.AddSingleton<IChangeRequestWriter>(sp => sp.GetRequiredService<ChangeRequestService>());

builder.Services.AddSingleton<CmdbCiService>();
builder.Services.AddSingleton<ICmdbCiReader>(sp => sp.GetRequiredService<CmdbCiService>());

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<IUserReader>(sp => sp.GetRequiredService<UserService>());

builder.Services.AddSingleton<ProblemService>();
builder.Services.AddSingleton<IProblemReader>(sp => sp.GetRequiredService<ProblemService>());
builder.Services.AddSingleton<IProblemWriter>(sp => sp.GetRequiredService<ProblemService>());

builder.Services.AddSingleton<ServiceCatalogService>();
builder.Services.AddSingleton<IServiceCatalogReader>(sp => sp.GetRequiredService<ServiceCatalogService>());

// Helpers
builder.Services.AddSingleton<ReferenceFieldResolver>();

// ─── Configure MCP HTTP Server ────────────────────────────────────
builder.ConfigureKestrelForMcp();

builder.Services.AddMcpHttpServer(options =>
{
    options.ServerName = "servicenow-enterprise-mcp";
    options.ServerVersion = "1.0.0";
    options.Endpoint = "/mcp";
    options.UseDefaultCors = true;
    options.EnableLogging = true;
});

// ─── Build & Initialize ──────────────────────────────────────────
var app = builder.Build();

// Initialize the static service locator for MCP tool handler access
ServiceLocator.Initialize(app.Services);

app.UseCors("McpCors");
app.MapMcpHttpServer();

app.Run("http://localhost:5020");
