# Phase 1 — Azure Setup Guide
## App Service + Azure SQL Database

> Follow every step manually in order. This is intentional — doing it yourself is how you learn.
> After each step, paste the output values into `context/project-context.md`.

---

## What You Are Creating

```
Azure Subscription
└── Resource Group: rg-learn-phase1
    ├── App Service Plan: plan-taskmanager-dev  (F1 Free)
    ├── Web App: taskmanager-api-<yourname>     (runs your .NET API)
    └── SQL Server: sql-taskmanager-dev
        └── SQL Database: sqldb-tasks           (Serverless — auto-pauses)
```

**Estimated cost:** $0–2/month (serverless SQL auto-pauses when idle)

---

## Prerequisites

Open a terminal (Command Prompt, PowerShell, or bash) and verify:

```bash
az --version        # Should show 2.x or higher
dotnet --version    # Should show 8.x.x
```

If Azure CLI is not installed: https://learn.microsoft.com/cli/azure/install-azure-cli

---

## Step 1 — Login to Azure

```bash
az login
```

Your browser opens. Sign in with your Azure account.
After login, run:

```bash
az account list --output table
```

You will see a table of subscriptions. Find your free account and run:

```bash
az account set --subscription "YOUR SUBSCRIPTION NAME OR ID"
az account show --output table
```

✅ Confirm: the correct subscription is shown as `IsDefault = True`.

**Copy to `context/project-context.md`:**
- Subscription ID
- Tenant ID

---

## Step 2 — Set a Budget Alert (Do This Once)

Prevents surprise charges on your free account.

```bash
az consumption budget create \
  --budget-name "azure-learning-budget" \
  --amount 20 \
  --time-grain Monthly \
  --category Cost
```

> You will get an email if you approach $20/month spend.

---

## Step 3 — Create Resource Group

A resource group is a logical container. Deleting it deletes everything inside.

```bash
az group create \
  --name rg-learn-phase1 \
  --location eastus
```

Expected output: `"provisioningState": "Succeeded"`

**Why eastus?** It has the widest free-tier availability. You can use `westeurope` or `southeastasia` if you prefer.

---

## Step 4 — Create App Service Plan (Free F1 Tier)

The plan defines the compute power for your hosted apps.

```bash
az appservice plan create \
  --name plan-taskmanager-dev \
  --resource-group rg-learn-phase1 \
  --sku F1 \
  --is-linux
```

**F1 tier gives you:**
- 60 CPU minutes per day
- 1 GB memory
- Shared infrastructure (not dedicated)
- No custom domain, no SSL offloading
- Perfect for learning

---

## Step 5 — Create the Web App

> App Service names must be **globally unique** across all of Azure.
> Use something like: `taskmanager-api-john-2024`

```bash
az webapp create \
  --name taskmanager-api-YOURNAME \
  --resource-group rg-learn-phase1 \
  --plan plan-taskmanager-dev \
  --runtime "DOTNETCORE:8.0"
```

After it completes, open your browser and visit:
```
https://taskmanager-api-YOURNAME.azurewebsites.net
```

You should see: *"Your web app is running and waiting for your content."*

**Copy to `context/project-context.md`:**
- App Service Name
- App Service URL

---

## Step 6 — Create Azure SQL Server

This creates the logical SQL server (not a VM — it's a managed service).

```bash
az sql server create \
  --name sql-taskmanager-dev \
  --resource-group rg-learn-phase1 \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "AzLearn@2024!Secure"
```

> **Password rules:** Uppercase + lowercase + number + special character, min 12 chars.
> Save this password somewhere safe — you'll need it.

**Copy to `context/project-context.md`:**
- SQL Admin Password (keep private!)

---

## Step 7 — Create SQL Database (Serverless)

```bash
az sql db create \
  --resource-group rg-learn-phase1 \
  --server sql-taskmanager-dev \
  --name sqldb-tasks \
  --edition GeneralPurpose \
  --compute-model Serverless \
  --family Gen5 \
  --capacity 1 \
  --auto-pause-delay 60
```

**What Serverless means:**
- Auto-pauses after 60 minutes of no activity → **$0 cost when paused**
- Wakes up automatically when your app connects (takes ~15–30 seconds first time)
- Charges only for the seconds it is actually computing
- Perfect for learning — you won't be billed overnight

---

## Step 8 — Configure Firewall Rules

Azure SQL blocks all connections by default. You need two rules:

```bash
# Rule 1: Allow Azure services (your App Service → SQL)
az sql server firewall-rule create \
  --resource-group rg-learn-phase1 \
  --server sql-taskmanager-dev \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Rule 2: Allow YOUR local machine (for running EF migrations)
MY_IP=$(curl -s https://api.ipify.org)
echo "Your IP: $MY_IP"

az sql server firewall-rule create \
  --resource-group rg-learn-phase1 \
  --server sql-taskmanager-dev \
  --name AllowMyLocalIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

> If you change networks (e.g. go to a coffee shop), run Rule 2 again with the new IP.

---

## Step 9 — Get the Connection String

```bash
az sql db show-connection-string \
  --server sql-taskmanager-dev \
  --name sqldb-tasks \
  --client ado.net
```

This outputs a template. Replace `<username>` and `<password>` with yours:

```
Server=tcp:sql-taskmanager-dev.database.windows.net,1433;
Initial Catalog=sqldb-tasks;
Persist Security Info=False;
User ID=sqladmin;
Password=AzLearn@2024!Secure;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

**Copy this connection string to `context/project-context.md`.**

---

## Step 10 — Set Connection String on App Service

This makes it available to your deployed API as an environment variable.

```bash
az webapp config connection-string set \
  --resource-group rg-learn-phase1 \
  --name taskmanager-api-YOURNAME \
  --settings DefaultConnection="Server=tcp:sql-taskmanager-dev.database.windows.net,1433;Initial Catalog=sqldb-tasks;Persist Security Info=False;User ID=sqladmin;Password=AzLearn@2024!Secure;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
  --connection-string-type SQLAzure
```

---

## Step 11 — Verify Everything in Azure Portal

1. Go to https://portal.azure.com
2. Click **Resource groups** → `rg-learn-phase1`
3. You should see: App Service Plan, App Service, SQL Server, SQL Database
4. Click the **App Service** → check Status = Running
5. Click the **SQL Database** → check Status = Online

---

## ✅ Phase 1 Azure Setup Checklist

- [ ] Logged in: `az account show`
- [ ] Budget alert created
- [ ] Resource group `rg-learn-phase1` created
- [ ] App Service Plan (F1) created
- [ ] Web App created — URL works in browser
- [ ] SQL Server created
- [ ] SQL Database created (Serverless, auto-pause 60 min)
- [ ] Firewall rules added (Azure + your IP)
- [ ] Connection string saved in `context/project-context.md`
- [ ] Connection string set on App Service

---

## What to Explore in Azure Portal

After setup, spend 10 minutes exploring:

**App Service:**
- Overview → see URL, status, plan
- Configuration → Application Settings and Connection Strings
- Monitoring → Log stream (empty for now — will fill when you deploy)
- Deployment → Deployment Center

**SQL Database:**
- Overview → status, server name, pricing tier
- Query editor (preview) → login and run `SELECT 1` to confirm it works
- Backups → see automatic backup schedule

---

## Now Start the Project

✅ Azure resources are ready.

Tell Claude Code:
```
Start Phase 1 — create the Task Manager API project
```

Claude Code will scaffold the project using the values from `context/project-context.md`.

---

## Cleanup (Run When Done With Phase 1)

```bash
az group delete --name rg-learn-phase1 --yes --no-wait
```

This deletes everything. Takes 3–5 minutes in the background.
