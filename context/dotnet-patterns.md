# .NET Patterns Reference

> Used by Claude Code when generating or reviewing code in any phase.

---

## Project Setup Commands

```bash
# New Web API
dotnet new webapi -n ProjectName
dotnet new sln -n SolutionName
dotnet sln SolutionName.sln add ProjectName/ProjectName.csproj

# Azure Functions (isolated worker)
func init FunctionApp --worker-runtime dotnet-isolated
cd FunctionApp
func new --name MyFunction --template "HTTP trigger"

# Test project
dotnet new xunit -n ProjectName.Tests
dotnet sln SolutionName.sln add ProjectName.Tests/ProjectName.Tests.csproj
dotnet add ProjectName.Tests/ProjectName.Tests.csproj reference ProjectName/ProjectName.csproj

# EF Core tools (install once globally)
dotnet tool install --global dotnet-ef
```

---

## EF Core Migration Commands

```bash
# Create a migration
dotnet ef migrations add InitialCreate --project src/ProjectName

# Apply to local DB
dotnet ef database update --project src/ProjectName

# Apply to Azure SQL (via env var — never hardcode)
$env:ConnectionStrings__DefaultConnection = "your-azure-sql-conn-str"  # PowerShell
export ConnectionStrings__DefaultConnection="your-azure-sql-conn-str"  # bash
dotnet ef database update --project src/ProjectName

# Remove last migration (if not yet applied)
dotnet ef migrations remove --project src/ProjectName

# Generate SQL script (for production review)
dotnet ef migrations script --project src/ProjectName -o migrations.sql

# List applied migrations
dotnet ef migrations list --project src/ProjectName
```

---

## HTTP Status Codes — Convention

| Scenario | Code | Method |
|----------|------|--------|
| Read success | 200 OK | `Ok(data)` |
| Created | 201 Created | `CreatedAtAction(...)` |
| Updated | 200 OK | `Ok(data)` |
| Deleted | 204 No Content | `NoContent()` |
| Not found | 404 Not Found | `NotFound()` |
| Invalid input | 400 Bad Request | `BadRequest(ModelState)` |
| Auth required | 401 Unauthorized | automatic via `[Authorize]` |
| No permission | 403 Forbidden | automatic via policy |
| Conflict | 409 Conflict | `Conflict(new { error = "..." })` |
| Server error | 500 | via error middleware |

---

## CancellationToken — Always Pass Through

```csharp
// Controller — ASP.NET Core injects it automatically
[HttpGet]
public async Task<IActionResult> GetAll(CancellationToken ct)
    => Ok(await _service.GetAllAsync(ct));

// Service — receive and pass to EF Core
public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    => await _db.Items.AsNoTracking().ToListAsync(ct);
```

---

## Options Pattern — Strongly-Typed Config

```csharp
// Options class
public sealed class CosmosOptions
{
    public const string Section = "CosmosDb";
    [Required] public required string DatabaseName { get; init; }
    [Required] public required string ContainerName { get; init; }
}

// Register in Program.cs
builder.Services.AddOptions<CosmosOptions>()
    .BindConfiguration(CosmosOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();   // Fails fast on startup if config is missing

// Inject in service
public sealed class MyService(IOptions<CosmosOptions> options)
{
    private readonly CosmosOptions _opts = options.Value;
}
```

---

## Validation Attributes

```csharp
public sealed class CreateTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    [MinLength(3)]
    public required string Title { get; init; }

    [Range(1, 3, ErrorMessage = "Priority must be 1, 2, or 3")]
    public int Priority { get; init; } = 2;

    [EmailAddress]
    public string? AssigneeEmail { get; init; }

    [Url]
    public string? ReferenceUrl { get; init; }
}
```

---

## Pagination Pattern

```csharp
// Request
public sealed class PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip => (Page - 1) * PageSize;
}

// Response
public sealed class PagedResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
}

// Service usage
var total = await query.CountAsync(ct);
var items = await query.Skip(req.Skip).Take(req.PageSize).ToListAsync(ct);
```

---

## Health Checks

```csharp
// Register
builder.Services.AddHealthChecks()
    .AddSqlServer(connStr, name: "sql")
    .AddAzureBlobStorage(storageConn, name: "storage")
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Map
app.MapHealthChecks("/health");
```

---

## Useful Keyboard Shortcuts in Visual Studio

| Action | Shortcut |
|--------|----------|
| Run with debugger | F5 |
| Run without debugger | Ctrl+F5 |
| Stop debugging | Shift+F5 |
| Set breakpoint | F9 |
| Go to definition | F12 |
| Find all references | Shift+F12 |
| Quick fix / refactor | Ctrl+. |
| Build solution | Ctrl+Shift+B |
| Open NuGet Manager | Right-click project → Manage NuGet |
| Package Manager Console | Tools → NuGet → PMC |

---

## Visual Studio Tips for This Project

- **Solution Explorer**: Right-click solution → Add → Existing Project to add `.csproj`
- **Connected Services**: Right-click project → Add → Connected Services → Azure SQL, App Insights etc.
- **Publish**: Right-click project → Publish → Azure → App Service
- **Secrets Manager**: Right-click project → Manage User Secrets (alternative to `appsettings.Development.json`)
- **HTTP Files**: Add `.http` files to test endpoints without Postman (VS 2022 supports REST Client)
