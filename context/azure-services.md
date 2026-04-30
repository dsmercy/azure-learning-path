# Azure Services — SDK & CLI Reference

> Claude Code uses this when writing Azure SDK code and CLI commands.

---

## NuGet Packages Per Phase

| Phase | Package | Use |
|-------|---------|-----|
| 1 | `Microsoft.EntityFrameworkCore.SqlServer` | EF Core → Azure SQL |
| 1 | `Microsoft.EntityFrameworkCore.Design` | EF Core migrations |
| 1 | `AspNetCore.HealthChecks.SqlServer` | SQL health check |
| 2 | `Azure.Storage.Blobs` | Blob upload/download |
| 2 | `Microsoft.Azure.Cosmos` | Cosmos DB SDK |
| 3 | `Microsoft.Azure.Functions.Worker` | Functions isolated worker |
| 3 | `Microsoft.Azure.Functions.Worker.Extensions.*` | Trigger extensions |
| 4 | `Azure.Messaging.ServiceBus` | Service Bus queues/topics |
| 4 | `Azure.Messaging.EventGrid` | Event Grid publisher |
| 4 | `Azure.Messaging.EventHubs` | Event Hubs producer/consumer |
| 5 | `Microsoft.Identity.Web` | Azure AD JWT auth |
| 5 | `Azure.Identity` | DefaultAzureCredential |
| 5 | `Azure.Security.KeyVault.Secrets` | Read secrets from KV |
| 5 | `Azure.Extensions.AspNetCore.Configuration.Secrets` | Load KV into IConfiguration |
| 6 | `Microsoft.ApplicationInsights.AspNetCore` | App Insights |
| 9 | All of the above | Capstone |

---

## SDK Client Lifetime Rules

| Client | Register As | Why |
|--------|-------------|-----|
| `BlobServiceClient` | **Singleton** | Thread-safe, manages HTTP connections |
| `CosmosClient` | **Singleton** | Manages connection pool |
| `ServiceBusClient` | **Singleton** | Manages AMQP connections |
| `SecretClient` (Key Vault) | **Singleton** | Caches auth tokens |
| `EventGridPublisherClient` | **Singleton** | Stateless HTTP |
| `EventHubProducerClient` | **Singleton** | Manages partition state |
| `AppDbContext` (EF Core) | **Scoped** | Per-request, not thread-safe |

---

## Connection String Formats

### Azure SQL
```
Server=tcp:{server}.database.windows.net,1433;
Initial Catalog={database};
Persist Security Info=False;
User ID={user};Password={password};
Encrypt=True;TrustServerCertificate=False;
Connection Timeout=30;
```

### Blob Storage
```
DefaultEndpointsProtocol=https;AccountName={name};
AccountKey={key};EndpointSuffix=core.windows.net
```

### Cosmos DB
```
AccountEndpoint=https://{name}.documents.azure.com:443/;AccountKey={key};
```

### Service Bus
```
Endpoint=sb://{namespace}.servicebus.windows.net/;
SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={key}
```

### Event Hubs
```
Endpoint=sb://{namespace}.servicebus.windows.net/;
SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={key};
EntityPath={hub-name}
```

---

## DefaultAzureCredential — How It Works

```
When running LOCALLY:
  → Tries Azure CLI credentials  (az login)
  → Tries Visual Studio credentials

When running IN AZURE (App Service with Managed Identity ON):
  → Tries Managed Identity  ← this fires automatically, no password needed!
```

Use it everywhere:
```csharp
new SecretClient(new Uri(kvUrl), new DefaultAzureCredential())
```

---

## Azure CLI Quick Reference

### Login & Account
```bash
az login                                     # Opens browser to sign in
az account list --output table               # List your subscriptions
az account set --subscription "NAME-OR-ID"   # Switch subscription
az account show --output table               # Confirm current subscription
```

### Resource Groups
```bash
az group create --name rg-name --location centralus
az group list --output table
az group show --name rg-name
az group delete --name rg-name --yes --no-wait    # Async delete
```

### App Service
```bash
# Create plan
az appservice plan create --name plan-name --resource-group rg-name --sku F1 --is-linux

# Create web app
az webapp create --name app-name --resource-group rg-name \
  --plan plan-name --runtime "DOTNETCORE:8.0"

# Set app settings
az webapp config appsettings set --name app-name --resource-group rg-name \
  --settings KEY=VALUE KEY2=VALUE2

# Set connection strings
az webapp config connection-string set --name app-name --resource-group rg-name \
  --settings DefaultConnection="conn-str" --connection-string-type SQLAzure

# Deploy zip
az webapp deployment source config-zip --name app-name --resource-group rg-name --src app.zip

# View logs
az webapp log tail --name app-name --resource-group rg-name

# Restart
az webapp restart --name app-name --resource-group rg-name

# Enable Managed Identity
az webapp identity assign --name app-name --resource-group rg-name
```

### Azure SQL
```bash
# Create server
az sql server create --name sql-name --resource-group rg-name \
  --location centralus --admin-user sqladmin --admin-password "Pass@word123!"

# Create database (serverless — cheapest for learning)
az sql db create --resource-group rg-name --server sql-name --name db-name \
  --edition GeneralPurpose --compute-model Serverless \
  --family Gen5 --capacity 1 --auto-pause-delay 60

# Add firewall — allow Azure services
az sql server firewall-rule create --resource-group rg-name --server sql-name \
  --name AllowAzure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Add firewall — your local IP
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create --resource-group rg-name --server sql-name \
  --name AllowMyIP --start-ip-address $MY_IP --end-ip-address $MY_IP

# Get connection string
az sql db show-connection-string --server sql-name --name db-name --client ado.net
```

### Blob Storage
```bash
# Create storage account (name: lowercase+numbers, no hyphens, globally unique)
az storage account create --name stname --resource-group rg-name \
  --location centralus --sku Standard_LRS

# Get connection string
az storage account show-connection-string --name stname --resource-group rg-name -o tsv

# Create container
STORAGE_KEY=$(az storage account keys list --resource-group rg-name \
  --account-name stname --query "[0].value" -o tsv)
az storage container create --name uploads --account-name stname \
  --account-key $STORAGE_KEY --public-access off
```

### Cosmos DB
```bash
# Create account (serverless)
az cosmosdb create --name cosmos-name --resource-group rg-name \
  --kind GlobalDocumentDB --capabilities EnableServerless \
  --locations regionName=centralus failoverPriority=0

# Create database
az cosmosdb sql database create --account-name cosmos-name \
  --resource-group rg-name --name MyDatabase

# Create container
az cosmosdb sql container create --account-name cosmos-name \
  --resource-group rg-name --database-name MyDatabase \
  --name MyContainer --partition-key-path "/partitionKey"

# Get connection string
az cosmosdb keys list --name cosmos-name --resource-group rg-name \
  --type connection-strings --query "connectionStrings[0].connectionString" -o tsv
```

### Key Vault
```bash
# Create
az keyvault create --name kv-name --resource-group rg-name \
  --location centralus --enable-rbac-authorization true

# Get your Object ID
MY_OID=$(az ad signed-in-user show --query id -o tsv)

# Grant yourself Secrets Officer (so you can add secrets)
KV_ID=$(az keyvault show --name kv-name --query id -o tsv)
az role assignment create --assignee $MY_OID \
  --role "Key Vault Secrets Officer" --scope $KV_ID

# Add secrets
az keyvault secret set --vault-name kv-name --name "SecretName" --value "SecretValue"

# List secret names
az keyvault secret list --vault-name kv-name --query "[].name" -o table

# Grant App Service Managed Identity access to read secrets
MI_OID=$(az webapp identity show --name app-name --resource-group rg-name \
  --query principalId -o tsv)
az role assignment create --assignee $MI_OID \
  --role "Key Vault Secrets User" --scope $KV_ID
```

### Azure Functions
```bash
# Create Function App
az functionapp create --name func-name --resource-group rg-name \
  --storage-account stname --consumption-plan-location centralus \
  --runtime dotnet-isolated --runtime-version 8 --functions-version 4

# Deploy
func azure functionapp publish func-name --dotnet-isolated

# View logs
az functionapp deployment list --name func-name --resource-group rg-name
```

### Application Insights
```bash
# Create Log Analytics workspace first
az monitor log-analytics workspace create \
  --workspace-name law-name --resource-group rg-name --location centralus

LAW_ID=$(az monitor log-analytics workspace show \
  --workspace-name law-name --resource-group rg-name --query id -o tsv)

# Create App Insights connected to workspace
az monitor app-insights component create \
  --app ai-name --resource-group rg-name --location centralus \
  --workspace $LAW_ID --kind web

# Get connection string
az monitor app-insights component show \
  --app ai-name --resource-group rg-name \
  --query connectionString -o tsv
```

### Service Bus
```bash
# Create namespace (Basic = queues only; Standard = + topics)
az servicebus namespace create --name sbns-name --resource-group rg-name \
  --location centralus --sku Basic

# Create queue
az servicebus queue create --name my-queue \
  --namespace-name sbns-name --resource-group rg-name

# Get connection string
az servicebus namespace authorization-rule keys list \
  --resource-group rg-name --namespace-name sbns-name \
  --name RootManageSharedAccessKey --query primaryConnectionString -o tsv
```
