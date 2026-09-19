# ServiceNow Enterprise MCP Server

A SOLID-compliant C# .NET 10 Model Context Protocol (MCP) server that interfaces securely with the ServiceNow REST Table API via Streamable HTTP transport.

Built with [McpHttpServer](https://www.nuget.org/packages/McpHttpServer/) — a .NET library for hosting MCP servers using the Streamable HTTP transport protocol.

## Features

- ✅ **Streamable HTTP Transport** — Runs as an independent network daemon (not stdio)
- ✅ **OAuth 2.0 Client Credentials** — Secure server-to-server authentication with auto-refresh
- ✅ **SOLID Architecture** — Interface Segregation, Single Responsibility, Dependency Injection
- ✅ **Reference Field Resolution** — Accepts display names (e.g., CI name) and resolves to sys_id
- ✅ **20+ MCP Tools** — Full CRUD for Incidents, Change Requests, Problems, CMDB, Users, Service Catalog
- ✅ **MCP Resources** — URI-based read-only access to reports and schema
- ✅ **MCP Prompts** — Guided workflows for incident triage and change management
- ✅ **Attribute-Based Auto-Discovery** — No switch statements or manual routing

## Available Tools (20+)

| Category | Tools |
|----------|-------|
| **Incidents** | `get_incident`, `query_incidents`, `create_incident`, `update_incident`, `add_incident_comment` |
| **Change Requests** | `get_change_request`, `query_change_requests`, `create_change_request`, `update_change_request` |
| **CMDB CI** | `get_cmdb_ci`, `query_cmdb_cis` |
| **Users** | `get_user`, `query_users` |
| **Problems** | `get_problem`, `query_problems`, `create_problem`, `update_problem` |
| **Service Catalog** | `get_sc_request`, `query_sc_requests` |

## Resources

| URI | Description |
|-----|-------------|
| `servicenow://reports/active_incidents` | Summary of active incidents |
| `servicenow://reports/open_changes` | Summary of open change requests |
| `servicenow://schema/tables` | Available table schemas and key fields |

## Prompts

| Name | Description |
|------|-------------|
| `servicenow-triage_incident` | Guided incident triage workflow |
| `servicenow-create_change` | Guided change request creation |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- ServiceNow instance with OAuth 2.0 configured
- OAuth Application Registry entry with Client Credentials grant enabled

## ServiceNow OAuth Setup

1. Navigate to **System OAuth > Application Registry**
2. Click **New** → **Create an OAuth API endpoint for external clients**
3. Configure:
   - **Name**: `MCP_ServiceNow_Integration`
   - **Default Grant Type**: `Client Credentials`
   - **OAuth Application User**: Select a service account with `itil` and `cmdb_read` roles
4. Enable the system property: `glide.oauth.inbound.client.credential.grant_type.enabled = true`

## Configuration

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `SERVICENOW_INSTANCE_URL` | ✅ | Instance URL (e.g., `https://dev12345.service-now.com`) |
| `SERVICENOW_CLIENT_ID` | ✅ | OAuth Client ID |
| `SERVICENOW_CLIENT_SECRET` | ✅ | OAuth Client Secret |
| `SERVICENOW_USERNAME` | Optional | For Resource Owner Password grant fallback |
| `SERVICENOW_PASSWORD` | Optional | For Resource Owner Password grant fallback |

### appsettings.json

```json
{
  "ServiceNow": {
    "InstanceUrl": "https://dev12345.service-now.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

## Building & Running

```bash
# Build
dotnet build -c Release

# Run (listens on http://localhost:5020/mcp)
dotnet run
```

## MCP Client Integration

### Antigravity / Claude Desktop / VS Code / Cursor

Since this server uses **Streamable HTTP transport**, configure clients to connect to the HTTP endpoint:

```json
{
  "mcpServers": {
    "servicenow": {
      "transport": "http",
      "url": "http://localhost:5020/mcp"
    }
  }
}
```

## Architecture

```
Service_Now_MCP/
├── ServiceNow_MCP.slnx                 # Visual Studio solution file
├── ServiceNow_MCP.csproj               # Project file (targets net10.0)
├── Program.cs                          # Entry point + DI registration
├── Configuration/
│   └── ServiceNowConfiguration.cs      # Env var + appsettings loader
├── Auth/
│   ├── ITokenProvider.cs               # Token provider interface
│   └── OAuthClientCredentialsProvider.cs # OAuth 2.0 Client Credentials
├── Clients/
│   ├── IServiceNowHttpClient.cs        # HTTP client interface
│   └── ServiceNowHttpClient.cs         # Concrete HTTP client
├── Services/
│   ├── IIncidentReader.cs / IIncidentWriter.cs / IncidentService.cs
│   ├── IChangeRequestReader.cs / IChangeRequestWriter.cs / ChangeRequestService.cs
│   ├── ICmdbCiReader.cs / CmdbCiService.cs
│   ├── IUserReader.cs / UserService.cs
│   ├── IProblemReader.cs / IProblemWriter.cs / ProblemService.cs
│   └── IServiceCatalogReader.cs / ServiceCatalogService.cs
├── Helpers/
│   ├── ServiceLocator.cs               # DI bridge for static handlers
│   └── ReferenceFieldResolver.cs       # Display name → sys_id resolver
├── Tools/
│   ├── IncidentTools.cs
│   ├── ChangeRequestTools.cs
│   ├── CmdbCiTools.cs
│   ├── UserTools.cs
│   ├── ProblemTools.cs
│   └── ServiceCatalogTools.cs
├── Resources/
│   └── ServiceNowResources.cs
└── Prompts/
    └── ServiceNowPrompts.cs
```

## SOLID Principles Compliance

| Principle | Implementation |
|-----------|----------------|
| **SRP** | Separate classes for HTTP client, token management, each service domain |
| **OCP** | New tool handlers added via `[McpHandler]` attribute — no routing changes needed |
| **LSP** | All services are substitutable via their interfaces |
| **ISP** | Separate `IIncidentReader` / `IIncidentWriter` instead of bloated `IIncidentManager` |
| **DIP** | All dependencies injected via ASP.NET Core DI container |

## License

MIT License

## Author

Built with [McpHttpServer](https://www.nuget.org/packages/McpHttpServer/) by Keerthi RB
