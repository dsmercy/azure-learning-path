# Phase 9 — Azure Setup Guide
## Capstone Project — All Services Combined

---

## What You Are Creating

All services from Phases 1–8 combined into one project.

```
Resource Group: rg-learn-capstone
├── App Service Plan + Web App    (API host)
├── Azure SQL Database            (tasks, users, boards)
├── Cosmos DB                     (activity feed)
├── Blob Storage                  (file attachments)
├── Service Bus                   (notifications queue)
├── Function App                  (background processing)
├── Key Vault                     (ALL secrets stored here)
└── Application Insights          (full monitoring)
```

**Estimated cost:** $0–3/month — all free/serverless tiers.

---

## Prerequisites

Complete Phases 1–8 first. You should understand each service before combining them.

---

## Step 1 — Set Variables

Open a terminal and set these once. All subsequent commands use them.

```bash
RG="rg-learn-capstone"
LOCATION="eastus"
SUFFIX="YOUR-SUFFIX"   # e.g. your initials + 4 digits: jd2048
```

---

## Step 2 — Create Resource Group

```bash
az group create --name $RG --location $LOCATION
```

---

## Step 3 — App Service + Web App

```bash
az appservice plan create --name "plan-capstone" \
  --resource-group $RG --sku F1 --is-linux

az webapp create \
  --name "app-taskboard-$SUFFIX" \
  --resource-group $RG \
  --plan plan-capstone \
  --runtime "DOTNETCORE:8.0"

# Enable Managed Identity
az webapp identity assign \
  --name "app-taskboard-$SUFFIX" --resource-group $RG

API_MI_OID=$(az webapp identity show \
  --name "app-taskboard-$SUFFIX" --resource-group $RG \
  --query principalId -o tsv)
echo "Managed Identity OID: $API_MI_OID"
```

---

## Step 4 — Azure SQL

```bash
az sql server create \
  --name "sql-capstone-$SUFFIX" \
  --resource-group $RG \
  --location $LOCATION \
  --admin-user sqladmin \
  --admin-password "Cap2024@Secure!"

az sql db create \
  --resource-group $RG \
  --server "sql-capstone-$SUFFIX" \
  --name capstone-db \
  --edition GeneralPurpose \
  --compute-model Serverless \
  --family Gen5 --capacity 1 \
  --auto-pause-delay 60

az sql server firewall-rule create \
  --resource-group $RG \
  --server "sql-capstone-$SUFFIX" \
  --name AllowAzure \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group $RG \
  --server "sql-capstone-$SUFFIX" \
  --name AllowLocal \
  --start-ip-address $MY_IP --end-ip-address $MY_IP

SQL_CONN=$(az sql db show-connection-string \
  --server "sql-capstone-$SUFFIX" --name capstone-db \
  --client ado.net -o tsv | \
  sed "s/<username>/sqladmin/" | \
  sed "s/<password>/Cap2024@Secure!/")
echo "SQL Connection String: $SQL_CONN"
```

---

## Step 5 — Cosmos DB

```bash
az cosmosdb create \
  --name "cosmos-capstone-$SUFFIX" \
  --resource-group $RG \
  --kind GlobalDocumentDB \
  --capabilities EnableServerless \
  --locations regionName=$LOCATION failoverPriority=0

az cosmosdb sql database create \
  --account-name "cosmos-capstone-$SUFFIX" \
  --resource-group $RG \
  --name ActivityFeedDb

az cosmosdb sql container create \
  --account-name "cosmos-capstone-$SUFFIX" \
  --resource-group $RG \
  --database-name ActivityFeedDb \
  --name Activities \
  --partition-key-path "/boardId"

COSMOS_CONN=$(az cosmosdb keys list \
  --name "cosmos-capstone-$SUFFIX" --resource-group $RG \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" -o tsv)
echo "Cosmos Connection: $COSMOS_CONN"
```

---

## Step 6 — Blob Storage

```bash
az storage account create \
  --name "stcapstone${SUFFIX}" \
  --resource-group $RG --location $LOCATION --sku Standard_LRS

STORAGE_KEY=$(az storage account keys list \
  --resource-group $RG \
  --account-name "stcapstone${SUFFIX}" \
  --query "[0].value" -o tsv)

az storage container create \
  --name attachments \
  --account-name "stcapstone${SUFFIX}" \
  --account-key $STORAGE_KEY \
  --public-access off

STORAGE_CONN=$(az storage account show-connection-string \
  --resource-group $RG --name "stcapstone${SUFFIX}" -o tsv)
echo "Storage Connection: $STORAGE_CONN"
```

---

## Step 7 — Service Bus

```bash
az servicebus namespace create \
  --name "sbns-capstone-$SUFFIX" \
  --resource-group $RG --location $LOCATION --sku Basic

az servicebus queue create \
  --name notifications-queue \
  --namespace-name "sbns-capstone-$SUFFIX" --resource-group $RG

SB_CONN=$(az servicebus namespace authorization-rule keys list \
  --resource-group $RG \
  --namespace-name "sbns-capstone-$SUFFIX" \
  --name RootManageSharedAccessKey \
  --query "primaryConnectionString" -o tsv)
echo "Service Bus Connection: $SB_CONN"
```

---

## Step 8 — Azure Functions

```bash
az storage account create \
  --name "stfunccap${SUFFIX}" \
  --resource-group $RG --location $LOCATION --sku Standard_LRS

az functionapp create \
  --name "func-capstone-$SUFFIX" \
  --resource-group $RG \
  --storage-account "stfunccap${SUFFIX}" \
  --consumption-plan-location $LOCATION \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4
```

---

## Step 9 — Key Vault and Store All Secrets

```bash
az keyvault create \
  --name "kv-capstone-$SUFFIX" \
  --resource-group $RG --location $LOCATION \
  --enable-rbac-authorization true

KV_ID=$(az keyvault show --name "kv-capstone-$SUFFIX" --query id -o tsv)
MY_OID=$(az ad signed-in-user show --query id -o tsv)

# Grant yourself access
az role assignment create --assignee $MY_OID \
  --role "Key Vault Secrets Officer" --scope $KV_ID

# Store ALL connection strings in Key Vault
az keyvault secret set --vault-name "kv-capstone-$SUFFIX" \
  --name "SqlConnectionString"     --value "$SQL_CONN"
az keyvault secret set --vault-name "kv-capstone-$SUFFIX" \
  --name "CosmosConnectionString"  --value "$COSMOS_CONN"
az keyvault secret set --vault-name "kv-capstone-$SUFFIX" \
  --name "StorageConnectionString" --value "$STORAGE_CONN"
az keyvault secret set --vault-name "kv-capstone-$SUFFIX" \
  --name "ServiceBusConnection"    --value "$SB_CONN"

echo "All secrets stored in Key Vault!"

# Grant App Service Managed Identity read access
az role assignment create --assignee $API_MI_OID \
  --role "Key Vault Secrets User" --scope $KV_ID

# Set Key Vault URL on App Service
az webapp config appsettings set \
  --name "app-taskboard-$SUFFIX" --resource-group $RG \
  --settings KeyVaultUrl="https://kv-capstone-$SUFFIX.vault.azure.net/"
```

---

## Step 10 — Application Insights

```bash
az monitor log-analytics workspace create \
  --workspace-name "law-capstone" \
  --resource-group $RG --location $LOCATION

LAW_ID=$(az monitor log-analytics workspace show \
  --workspace-name law-capstone --resource-group $RG --query id -o tsv)

az monitor app-insights component create \
  --app "ai-capstone" --resource-group $RG \
  --location $LOCATION --workspace $LAW_ID --kind web

AI_CONN=$(az monitor app-insights component show \
  --app ai-capstone --resource-group $RG \
  --query connectionString -o tsv)

az keyvault secret set --vault-name "kv-capstone-$SUFFIX" \
  --name "AppInsightsConnection" --value "$AI_CONN"

echo "App Insights connection stored in Key Vault"
```

---

## Step 11 — Print Summary

```bash
echo ""
echo "================================================"
echo "  CAPSTONE INFRASTRUCTURE READY"
echo "================================================"
echo "  Resource Group : $RG"
echo "  API URL        : https://app-taskboard-$SUFFIX.azurewebsites.net"
echo "  Key Vault      : https://kv-capstone-$SUFFIX.vault.azure.net"
echo "  SQL Server     : sql-capstone-$SUFFIX.database.windows.net"
echo "  Cosmos Account : cosmos-capstone-$SUFFIX"
echo "  Storage        : stcapstone${SUFFIX}"
echo "  Service Bus    : sbns-capstone-$SUFFIX"
echo "  Function App   : func-capstone-$SUFFIX"
echo "================================================"
echo ""
echo "All connection strings are stored in Key Vault."
echo "App Service uses Managed Identity to read them."
echo "No passwords in code or config files."
```

---

## ✅ Phase 9 Setup Checklist

- [ ] All steps 1–11 completed without errors
- [ ] API App Service URL opens in browser
- [ ] All secrets visible in Key Vault (portal → Secrets)
- [ ] Managed Identity visible on App Service (portal → Identity)
- [ ] Summary printed with all resource names

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 9 — create the Team Task Board capstone project using all services
```

---

## Cleanup

```bash
az group delete --name rg-learn-capstone --yes --no-wait
echo "Capstone cleanup started. Takes 5-10 minutes."
```
