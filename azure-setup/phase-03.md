# Phase 3 — Azure Setup Guide
## Azure Functions (Serverless)

---

## Prerequisites — Install Functions Core Tools

Before creating Azure resources, install the local development tools:

```bash
# Install Azure Functions Core Tools (v4)
npm install -g azure-functions-core-tools@4 --unsafe-perm true

# Verify
func --version   # Should print 4.x.x

# Install Azurite (local Azure Storage emulator — replaces real storage locally)
npm install -g azurite
azurite --version
```

---

## What You Are Creating

```
Resource Group: rg-learn-phase3
├── Storage Account: stfunclearningXXXX  (required by Functions runtime)
│   ├── Queue: task-queue                (for Queue trigger demo)
│   └── Container: trigger-uploads       (for Blob trigger demo)
└── Function App: func-taskprocessing-XXXX  (Consumption plan — free)
```

**Estimated cost:** $0/month — Functions Consumption plan includes 1 million free executions/month.

---

## Step 1 — Create Resource Group

```bash
az group create --name rg-learn-phase3 --location eastus
```

---

## Step 2 — Create Storage Account

Functions runtime requires a storage account for coordination.

```bash
az storage account create \
  --name stfunclearningYOURNAME \
  --resource-group rg-learn-phase3 \
  --location eastus \
  --sku Standard_LRS
```

---

## Step 3 — Create Queue and Blob Container for Demos

```bash
STORAGE_KEY=$(az storage account keys list \
  --resource-group rg-learn-phase3 \
  --account-name stfunclearningYOURNAME \
  --query "[0].value" -o tsv)

# Queue for Queue trigger demo
az storage queue create \
  --name task-queue \
  --account-name stfunclearningYOURNAME \
  --account-key $STORAGE_KEY

# Container for Blob trigger demo
az storage container create \
  --name trigger-uploads \
  --account-name stfunclearningYOURNAME \
  --account-key $STORAGE_KEY

echo "Queue and container created"
```

---

## Step 4 — Get Storage Connection String

```bash
az storage account show-connection-string \
  --resource-group rg-learn-phase3 \
  --name stfunclearningYOURNAME -o tsv
```

**Copy to `context/project-context.md`.**

---

## Step 5 — Create Function App (Consumption Plan — Free)

```bash
az functionapp create \
  --name func-taskprocessing-YOURNAME \
  --resource-group rg-learn-phase3 \
  --storage-account stfunclearningYOURNAME \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4
```

**Consumption plan:**
- Pay per execution (first 1M/month = FREE)
- Scales automatically from zero
- "Cold start" = first execution after idle takes extra time (~1–5 seconds)

---

## Step 6 — Explore in Azure Portal

1. Go to portal.azure.com → Function App → your function app
2. Click **Functions** — empty for now (nothing deployed yet)
3. Click **App files** → see `host.json`
4. Click **Monitor** → where execution logs appear after deployment

---

## Understanding the Trigger Types (Theory Before Code)

| Trigger | Fires When | Example Use |
|---------|-----------|-------------|
| **HTTP** | Someone calls a URL | Lightweight REST endpoint, webhook receiver |
| **Timer** | A CRON schedule | Daily report, cleanup job, scheduled sync |
| **Blob** | A file is added/changed in Storage | Resize image, extract PDF text, virus scan |
| **Queue** | A message arrives in a Queue | Background email send, async order processing |
| **Service Bus** | A message arrives in Service Bus | Enterprise event processing |
| **Durable** | Orchestrated workflow | Multi-step processes with retries and waiting |

---

## ✅ Phase 3 Azure Setup Checklist

- [ ] Functions Core Tools installed: `func --version`
- [ ] Azurite installed: `azurite --version`
- [ ] Resource group `rg-learn-phase3` created
- [ ] Storage account created
- [ ] Queue `task-queue` created
- [ ] Container `trigger-uploads` created
- [ ] Storage connection string saved to `context/project-context.md`
- [ ] Function App created (Consumption plan)
- [ ] Function App URL saved to `context/project-context.md`

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 3 — create the Azure Functions project with all trigger types
```

---

## Cleanup

```bash
az group delete --name rg-learn-phase3 --yes --no-wait
```
