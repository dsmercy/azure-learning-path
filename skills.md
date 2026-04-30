# Skills — Reusable Code Patterns

> Claude Code uses this file as a reference when generating code for any phase.
> All patterns here follow the standards defined in CLAUDE.md.

---

## 1. Service Interface + Implementation Pattern

```csharp
// IExampleService.cs
public interface IExampleService
{
    Task<IEnumerable<ExampleResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ExampleResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ExampleResponse> CreateAsync(CreateExampleRequest request, CancellationToken ct = default);
    Task<ExampleResponse?> UpdateAsync(int id, UpdateExampleRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

// ExampleService.cs
public sealed class ExampleService : IExampleService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(AppDbContext db, ILogger<ExampleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ExampleResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Examples.AsNoTracking()
                              .FirstOrDefaultAsync(e => e.Id == id, ct);
        return entity is null ? null : ExampleResponse.FromEntity(entity);
    }
}
```

---

## 2. Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ExamplesController : ControllerBase
{
    private readonly IExampleService _service;
    public ExamplesController(IExampleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExampleRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExampleRequest req, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, req, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
```

---

## 3. Entity / Request / Response DTO Pattern

```csharp
// Entity — maps to DB table
public sealed class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Request DTO — used as POST/PUT body
public sealed class CreateTaskRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }
    [MaxLength(1000)]
    public string? Description { get; init; }
}

// Response DTO — what the API returns (never return entity directly)
public sealed class TaskResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime CreatedAt { get; init; }

    public static TaskResponse FromEntity(TaskItem e) => new()
    {
        Id          = e.Id,
        Title       = e.Title,
        Description = e.Description,
        IsCompleted = e.IsCompleted,
        CreatedAt   = e.CreatedAt
    };
}
```

---

## 4. EF Core DbContext Pattern

```csharp
// AppDbContext.cs
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}

// TaskItemConfiguration.cs (separate file per entity)
public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.HasIndex(e => e.IsCompleted);
    }
}
```

---

## 5. Standard Program.cs Template

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" }));

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)));

builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "sql");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"); c.RoutePrefix = "swagger"; });
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => new { service = "My API", status = "running", docs = "/swagger" });

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.Run();
```

---

## 6. appsettings.json Template (committed — no secrets)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "PLACEHOLDER — Set in Azure App Service → Configuration",
    "AzureStorage":      "PLACEHOLDER — Set in Azure App Service → Configuration"
  },
  "KeyVaultUrl":  "PLACEHOLDER — https://your-kv.vault.azure.net/",
  "AzureAd": {
    "Instance":  "https://login.microsoftonline.com/",
    "TenantId":  "PLACEHOLDER",
    "ClientId":  "PLACEHOLDER"
  },
  "ApplicationInsights": {
    "ConnectionString": "PLACEHOLDER"
  }
}
```

---

## 7. appsettings.Development.json Template (NOT committed)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyAppDb;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

---

## 8. launchSettings.json Template (Visual Studio)

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 9. Blob Storage — Upload & SAS Token

```csharp
// Registration (singleton)
builder.Services.AddSingleton(new BlobServiceClient(
    builder.Configuration.GetConnectionString("AzureStorage")!));

// Upload
var container = _blobClient.GetBlobContainerClient("uploads");
await container.CreateIfNotExistsAsync();
var blob = container.GetBlobClient($"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
await blob.UploadAsync(file.OpenReadStream(),
    new BlobUploadOptions { HttpHeaders = new() { ContentType = file.ContentType } });

// SAS Token (time-limited download URL)
var sasUri = blob.GenerateSasUri(BlobSasPermissions.Read,
    DateTimeOffset.UtcNow.AddMinutes(60));
```

---

## 10. Azure Key Vault — Load at Startup

```csharp
// Loads ALL Key Vault secrets into IConfiguration
// Uses Managed Identity in Azure, Azure CLI credentials locally
var kvUrl = builder.Configuration["KeyVaultUrl"]!;
builder.Configuration.AddAzureKeyVault(new Uri(kvUrl), new DefaultAzureCredential());

// Now access secrets like normal config:
var dbConn = builder.Configuration["SqlConnectionString"]; // reads from KV
```

---

## 11. Service Bus — Send a Message

```csharp
// Registration (singleton)
builder.Services.AddSingleton(new ServiceBusClient(
    builder.Configuration.GetConnectionString("ServiceBus")!));

// Send
await using var sender = _serviceBusClient.CreateSender("my-queue");
var message = new ServiceBusMessage(JsonSerializer.Serialize(payload))
{
    MessageId   = Guid.NewGuid().ToString(),
    ContentType = "application/json"
};
await sender.SendMessageAsync(message);
```

---

## 12. Application Insights — Custom Telemetry

```csharp
// Registration
builder.Services.AddApplicationInsightsTelemetry(o =>
    o.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);

// Custom event
_telemetry.TrackEvent("TaskCreated", new Dictionary<string, string>
{
    ["TaskId"]   = task.Id.ToString(),
    ["Priority"] = task.Priority.ToString()
});

// Custom metric
_telemetry.TrackMetric("ProcessingDurationMs", stopwatch.Elapsed.TotalMilliseconds);
```

---

## 13. Global Error Handler Middleware

```csharp
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error   = "An unexpected error occurred",
                traceId = context.TraceIdentifier
            });
        }
    }
}

// In Program.cs — register before other middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
```

---

## 14. Useful dotnet CLI Commands

```bash
# Create project
dotnet new webapi -n ProjectName
dotnet new sln -n SolutionName
dotnet sln SolutionName.sln add ProjectName/ProjectName.csproj

# Packages
dotnet add package Azure.Storage.Blobs
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design

# EF Core
dotnet tool install --global dotnet-ef        # Install once
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run & publish
dotnet run --project src/ProjectName
dotnet publish -c Release -o ./publish
```
