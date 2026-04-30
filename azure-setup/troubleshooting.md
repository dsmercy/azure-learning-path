# Troubleshooting Guide
### Common errors across all phases and how to fix them

---

## Azure CLI Issues

### ❌ `az: command not found`
Azure CLI is not installed.
```bash
# Windows: download from https://learn.microsoft.com/cli/azure/install-azure-cli-windows
# macOS:
brew install azure-cli
# Linux:
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

### ❌ `Please run 'az login' to setup account`
```bash
az login
az account show   # confirm logged in
```

### ❌ `The subscription is not registered to use namespace 'Microsoft.Sql'`
Some resource providers need to be registered once:
```bash
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.DocumentDB
az provider register --namespace Microsoft.ServiceBus
az provider register --namespace Microsoft.EventGrid
az provider register --namespace Microsoft.EventHub
```

---

## App Service Issues

### ❌ "Service Unavailable" or "Application Error" after deploy

Step 1: Check the logs
```bash
az webapp log tail --resource-group rg-learn-phaseX --name YOUR-APP-NAME
```

Step 2: Common causes:
- **Missing connection string** → Add it in App Service → Configuration
- **App failed to start** → Check startup exception in logs
- **Wrong runtime** → Verify `--runtime "DOTNETCORE:8.0"` was used

Step 3: Restart the app
```bash
az webapp restart --resource-group rg-learn-phaseX --name YOUR-APP-NAME
```

### ❌ Deploy ZIP fails: "BadRequest"
Make sure you're zipping the **contents** of the publish folder, not the folder itself:
```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .    # Zip contents, not the folder
cd ..
az webapp deployment source config-zip --src deploy.zip ...
```

### ❌ App name already taken
App Service names are globally unique across all of Azure.
```bash
# Add more unique characters
az webapp create --name taskmanager-api-YOURNAME-2024 ...
```

---

## Azure SQL Issues

### ❌ "Cannot open server... firewall..."
Your IP is not in the firewall rules.
```bash
MY_IP=$(curl -s https://api.ipify.org)
echo "Your IP: $MY_IP"

az sql server firewall-rule create \
  --resource-group rg-learn-phaseX \
  --server sql-taskmanager-dev \
  --name AllowMyIP-$(date +%s) \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

### ❌ First API request is very slow (~30 seconds)
This is **expected** — your Serverless SQL database auto-paused and is waking up.
Subsequent requests will be fast. To disable auto-pause:
```bash
az sql db update \
  --resource-group rg-learn-phaseX \
  --server sql-taskmanager-dev \
  --name sqldb-tasks \
  --auto-pause-delay -1   # -1 = disabled
```

### ❌ EF Core migration fails: "Login failed for user"
Check your connection string has correct username/password:
```bash
# Verify server exists
az sql server show --name sql-taskmanager-dev --resource-group rg-learn-phaseX

# Test connection string manually with sqlcmd (if installed)
sqlcmd -S sql-taskmanager-dev.database.windows.net \
  -U sqladmin -P "YourPassword" \
  -d sqldb-tasks -Q "SELECT 1"
```

### ❌ "The model backing the context has changed"
Run pending migrations:
```bash
dotnet ef database update --project src/ProjectName
```

---

## Blob Storage Issues

### ❌ "AuthorizationFailure" when uploading
Your connection string is wrong or missing.
```bash
# Get fresh connection string
az storage account show-connection-string \
  --resource-group rg-learn-phaseX \
  --name YOUR-STORAGE-ACCOUNT -o tsv
```
Update `appsettings.Development.json` with the fresh value.

### ❌ SAS token returns 403 Forbidden
SAS token may have expired. Regenerate with longer expiry:
```csharp
var sasUri = blobClient.GenerateSasUri(
    BlobSasPermissions.Read,
    DateTimeOffset.UtcNow.AddHours(24));  // Increase expiry
```

---

## Cosmos DB Issues

### ❌ "Request rate is large" (429 Too Many Requests)
You're consuming more RUs than provisioned (Serverless has soft limits).
- Slow down test requests
- Check if cross-partition queries are running — these consume many RUs
- Add `?partitionKey=value` to queries when possible

### ❌ Cross-partition query is very slow
You're querying without the partition key. Fix:
```csharp
// Bad — scans all partitions
var query = new QueryDefinition("SELECT * FROM c WHERE c.name = @name");

// Good — specify partition key in options
var options = new QueryRequestOptions
{
    PartitionKey = new PartitionKey("Electronics")
};
```

---

## Azure Functions Issues

### ❌ `func: command not found`
```bash
npm install -g azure-functions-core-tools@4 --unsafe-perm true
func --version   # Verify
```

### ❌ Blob trigger not firing locally
Make sure Azurite is running:
```bash
azurite --silent --location c:\azurite  # Windows
azurite --silent --location /tmp/azurite # Mac/Linux
```
And `local.settings.json` has:
```json
"AzureWebJobsStorage": "UseDevelopmentStorage=true"
```

### ❌ Function deploys but doesn't appear in portal
Wait 2–3 minutes after deploy. If still missing:
```bash
# Restart the Function App
az functionapp restart --name YOUR-FUNC-APP --resource-group rg-learn-phaseX
```

---

## Key Vault Issues

### ❌ "Caller is not authorized to perform action on resource"
You don't have the right RBAC role:
```bash
MY_OID=$(az ad signed-in-user show --query id -o tsv)
KV_ID=$(az keyvault show --name YOUR-KV --query id -o tsv)

az role assignment create \
  --assignee $MY_OID \
  --role "Key Vault Secrets Officer" \
  --scope $KV_ID
```

### ❌ DefaultAzureCredential fails locally
Make sure you're logged in via Azure CLI:
```bash
az login
az account show   # Confirms active session
```
`DefaultAzureCredential` uses your `az login` session when running locally.

### ❌ App Service can't read Key Vault secrets after deploy
1. Confirm Managed Identity is ON:
   ```bash
   az webapp identity show --name YOUR-APP --resource-group rg-learn-phaseX
   ```
2. Confirm role assignment exists:
   ```bash
   az role assignment list --scope $KV_ID --output table
   ```
3. Confirm `KeyVaultUrl` is set in App Service settings:
   ```bash
   az webapp config appsettings list --name YOUR-APP --resource-group rg-learn-phaseX
   ```

---

## Service Bus Issues

### ❌ "Unauthorized access" when sending messages
Connection string is wrong. Get a fresh one:
```bash
az servicebus namespace authorization-rule keys list \
  --resource-group rg-learn-phaseX \
  --namespace-name YOUR-NAMESPACE \
  --name RootManageSharedAccessKey \
  --query "primaryConnectionString" -o tsv
```

### ❌ Messages not being consumed
Check the queue has messages and the processor is running:
```bash
# Check queue message count via portal
# Service Bus → Queues → YOUR-QUEUE → Overview → Active Message Count
```

---

## Visual Studio Issues

### ❌ "Could not find file ... appsettings.Development.json"
This file is in `.gitignore` — you need to create it manually.
Right-click project → Add → New Item → `appsettings.Development.json`
Or copy the template from `skills.md`.

### ❌ NuGet package restore fails
```bash
dotnet restore

# Or in VS: Tools → NuGet Package Manager → Package Manager Console
# PM> Update-Package -reinstall
```

### ❌ EF Core tools not found in Package Manager Console
```bash
# In Package Manager Console
Install-Package Microsoft.EntityFrameworkCore.Tools

# Or globally via CLI
dotnet tool install --global dotnet-ef
```

### ❌ Port already in use when pressing F5
Another instance is already running. In VS:
- Debug → Stop All (Shift+F5)
- Or change port in `Properties/launchSettings.json`

---

## Getting More Help

1. **Azure Status:** https://status.azure.com — check for outages
2. **Azure Docs:** https://learn.microsoft.com/azure
3. **Stack Overflow:** tag `azure` + specific service
4. **Ask Claude Code:** describe the error and paste the full stack trace
