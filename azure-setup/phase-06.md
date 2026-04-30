# Phase 6 — Azure Setup Guide
## Application Insights + Azure Monitor

---

## What You Are Creating

```
Resource Group: rg-learn-phase6
├── Log Analytics Workspace: law-learning    (central log store)
├── Application Insights: ai-taskmanager     (APM connected to workspace)
└── Action Group: ag-learning-alerts         (sends you email on alerts)
```

**Estimated cost:** $0/month — first 5 GB/month is always free.

---

## Step 1 — Create Resource Group

```bash
az group create --name rg-learn-phase6 --location eastus
```

---

## Step 2 — Create Log Analytics Workspace

```bash
az monitor log-analytics workspace create \
  --workspace-name law-learning \
  --resource-group rg-learn-phase6 \
  --location eastus \
  --sku PerGB2018
```

**What is Log Analytics?**
- A central store for logs from all your Azure services
- Queryable with KQL (Kusto Query Language)
- Application Insights, VMs, App Service, SQL, etc. can all send logs here
- Think of it as a cloud-scale log database

---

## Step 3 — Create Application Insights

```bash
# Get Log Analytics workspace ID
LAW_ID=$(az monitor log-analytics workspace show \
  --workspace-name law-learning \
  --resource-group rg-learn-phase6 \
  --query id -o tsv)

# Create Application Insights connected to the workspace
az monitor app-insights component create \
  --app ai-taskmanager \
  --resource-group rg-learn-phase6 \
  --location eastus \
  --workspace $LAW_ID \
  --kind web
```

**What is Application Insights?**
- Application Performance Monitoring (APM) for your .NET app
- With one NuGet package, it automatically tracks:
  - Every HTTP request (URL, duration, status code)
  - Database queries (SQL, Cosmos)
  - External HTTP calls your app makes
  - Exceptions and stack traces
  - Custom events and metrics you define
- Data flows into your Log Analytics Workspace

---

## Step 4 — Get Connection String

```bash
az monitor app-insights component show \
  --app ai-taskmanager \
  --resource-group rg-learn-phase6 \
  --query connectionString -o tsv
```

**Copy to `context/project-context.md`.**

This is different from an instrumentation key (the old approach). The connection string is the modern way.

---

## Step 5 — Create Action Group (Email Alerts)

```bash
az monitor action-group create \
  --name ag-learning-alerts \
  --resource-group rg-learn-phase6 \
  --short-name LrnAlert \
  --email-receiver name=Developer email-address=YOUR@EMAIL.COM
```

Replace `YOUR@EMAIL.COM` with your actual email.

An **Action Group** is a list of notification destinations. It can send to:
- Email
- SMS
- Webhook (e.g. Slack, Teams)
- Azure Function

---

## Step 6 — Explore in Azure Portal

**Application Insights (before deploying anything):**
1. portal.azure.com → Application Insights → ai-taskmanager
2. Overview → All metrics are at 0 (nothing deployed yet)
3. Explore the left menu — notice: Live Metrics, Failures, Performance, Logs

**After you deploy and generate traffic, come back and explore:**
- **Live Metrics** → real-time request and failure rates as they happen
- **Transaction Search** → find individual requests and trace them end-to-end
- **Performance** → slowest operations, dependency breakdown
- **Failures** → exceptions grouped by type
- **Logs** → write KQL queries against your telemetry

---

## KQL Queries to Try Later (After Deploying)

Open: **Application Insights → Logs**

```kusto
-- All requests in the last hour
requests
| where timestamp > ago(1h)
| summarize count() by name, resultCode
| order by count_ desc

-- Slow requests (over 500ms)
requests
| where duration > 500
| project timestamp, name, duration, resultCode
| order by duration desc

-- All exceptions
exceptions
| summarize count() by type
| order by count_ desc

-- Automatic SQL dependency tracking
dependencies
| where type == "SQL"
| project timestamp, name, duration, success
| order by duration desc
```

---

## ✅ Phase 6 Azure Setup Checklist

- [ ] Resource group `rg-learn-phase6` created
- [ ] Log Analytics workspace created
- [ ] Application Insights created (workspace-based)
- [ ] App Insights connection string saved to `context/project-context.md`
- [ ] Action group created with your email

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 6 — add Application Insights observability to the Task Manager API
```

---

## Cleanup

```bash
az group delete --name rg-learn-phase6 --yes --no-wait
```
