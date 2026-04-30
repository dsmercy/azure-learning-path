# Azure .NET Learning Path — Claude Code Instructions

> Claude Code reads this file automatically at the start of every session.
> Keep it updated as your project evolves.

---

## Project Purpose

Phase-by-phase learning of Azure services for a .NET full-stack developer.
Each phase has its own folder with a demo project and Azure setup instructions.

Developer works in **Visual Studio 2022**. Claude Code assists with:
- Scaffolding new projects when a phase begins
- Writing service, controller, and model code
- Running EF Core migrations
- Deploying to Azure via CLI
- Debugging and troubleshooting

---

## Phases Overview

| # | Phase | Azure Services | Status |
|---|-------|---------------|--------|
| 1 | App Service + Azure SQL | App Service, Azure SQL Database | ⬜ Not Started |
| 2 | Blob Storage + Cosmos DB | Blob Storage, Cosmos DB | ⬜ Not Started |
| 3 | Azure Functions | Functions (Consumption) | ⬜ Not Started |
| 4 | Messaging | Service Bus, Event Grid, Event Hubs | ⬜ Not Started |
| 5 | Security & Identity | Azure AD, RBAC, Managed Identity, Key Vault | ⬜ Not Started |
| 6 | Monitoring | Application Insights, Azure Monitor | ⬜ Not Started |
| 7 | Containers & AKS | Docker, ACI, AKS | ⬜ Not Started |
| 8 | DevOps & CI/CD | GitHub Actions, Azure DevOps | ⬜ Not Started |
| 9 | Capstone | All services integrated | ⬜ Not Started |

Update status to: ⬜ Not Started → 🔄 In Progress → ✅ Done

---

## How to Start a New Phase

1. Read `azure-setup/phase-XX.md` — create Azure resources manually first
2. Update `context/project-context.md` with the resource names you created
3. Tell Claude Code: **"Start Phase X — create the project"**
4. Claude Code will scaffold the project inside `Phase-0X-Name/src/`
5. Open the generated `.sln` in Visual Studio
6. Follow along, test, and learn

---

## Coding Standards (Always Apply)

### .NET Version & Language
- Target: **.NET 8**, C# 12
- Always enable: `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- Use file-scoped namespaces: `namespace MyApp.Services;`
- Use `required` for mandatory DTO properties
- Use primary constructors where it improves readability

### Project Architecture
```
Controller  →  IService (interface)  →  ServiceImpl  →  DbContext / SDK client
```
- Controllers: HTTP concerns only — no business logic
- Services: all business logic, injected via interface
- Models: three separate classes — `Entity` (DB), `Request` (input DTO), `Response` (output DTO)
- Never return EF entities directly from controllers — always map to a Response DTO

### Dependency Injection Registration
- `AddScoped` → services that use DbContext
- `AddSingleton` → Azure SDK clients (BlobServiceClient, CosmosClient, ServiceBusClient)
- `AddTransient` → lightweight stateless utilities

### Async Rules
- All I/O must be `async` — no `.Result` or `.Wait()`
- Method names end in `Async` when async
- Pass `CancellationToken ct = default` through all layers

### Error Handling
- Return correct HTTP status codes (200, 201, 204, 400, 404, 409, 500)
- Log with structured logging: `_logger.LogError(ex, "Failed for {Id}", id)`
- Never swallow exceptions silently

### Configuration & Secrets
- **Never hardcode secrets or connection strings**
- Local dev → `appsettings.Development.json` (always in `.gitignore`)
- Azure → App Service Configuration or Key Vault
- Committed `appsettings.json` contains only `"PLACEHOLDER"` values

### Logging
- Inject `ILogger<T>` wherever needed
- Use structured params: `_logger.LogInformation("Task {Id} created", task.Id)`

---

## File Naming Conventions

```
Models/
  TaskItem.cs           ← entity
  TaskRequest.cs        ← request DTO
  TaskResponse.cs       ← response DTO

Services/
  ITaskService.cs       ← interface
  TaskService.cs        ← implementation

Controllers/
  TasksController.cs    ← plural noun

Data/
  AppDbContext.cs
  Migrations/           ← auto-generated, never edit manually
```

---

## Git Conventions

Branch per phase:
```
main
└── phase/01-appservice
└── phase/02-storage
└── phase/03-functions
```

Commit format:
```
feat(phase1): add task CRUD endpoints
fix(phase2): correct SAS token expiry calculation
docs(phase3): update blob trigger notes
```

---

## Azure Free Tier Rules

Always use free/low-cost tiers during learning:

| Service | Use This Tier |
|---------|--------------|
| App Service | F1 (free) |
| Azure SQL | Serverless, auto-pause 60 min |
| Cosmos DB | Serverless |
| Functions | Consumption plan |
| Key Vault | Standard |
| App Insights | Default (5 GB/month free) |
| Service Bus | Basic (queues only, no topics) |
| AKS | 1 node, B2s — delete after learning! |

> ⚠️ Always run the cleanup commands after finishing a phase.

---

## Visual Studio Notes

- Each phase has a `.sln` file — open this in VS 2022
- Press `F5` to run with debugger, `Ctrl+F5` without
- Swagger UI opens automatically at `/swagger`
- `appsettings.Development.json` is for local connection strings (not committed)

---

## Reference Files

| File | Purpose |
|------|---------|
| `context/project-context.md` | Active phase, resource names, notes |
| `context/azure-services.md` | Azure SDK patterns and CLI reference |
| `context/dotnet-patterns.md` | Reusable .NET patterns |
| `skills.md` | Copy-paste code patterns |
| `azure-setup/phase-XX.md` | Step-by-step Azure resource creation |
