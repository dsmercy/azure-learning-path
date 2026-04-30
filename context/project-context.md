# Project Context

> Update this file as you progress. Claude Code reads it to know your current state.

---

## Current Status

```
Active Phase    : Phase 1 — App Service + Azure SQL
Phase Status    : ⬜ Not Started
Azure Logged In : No  (run: az login)
Subscription    : PLACEHOLDER — paste your subscription name here
```

---

## Your Azure Account

```
Subscription ID : PLACEHOLDER
Subscription Name: PLACEHOLDER
Tenant ID       : PLACEHOLDER
Region          : centralus   (change if you prefer another region)
```

To fill these in, run:
```bash
az login
az account show --output table
```

---

## Active Azure Resources

Update this section each time you create resources for a phase.
Claude Code uses these values when generating config files.

### Phase 1 — App Service + Azure SQL
```
Resource Group  : rg-learn-phase1
App Service Plan: plan-taskmanager-dev   (F1 — free)
App Service Name: PLACEHOLDER            (e.g. taskmanager-api-yourname-001)
App Service URL : https://PLACEHOLDER.azurewebsites.net
SQL Server Name : sql-taskmanager-dev
SQL Database    : sqldb-tasks
SQL Admin User  : sqladmin
SQL Admin Pass  : PLACEHOLDER            (never commit real password)
SQL Conn String : PLACEHOLDER
```

### Phase 2 — Blob Storage + Cosmos DB
```
Resource Group  : rg-learn-phase2
Storage Account : PLACEHOLDER            (stlearningXXXX — no hyphens)
Storage Conn    : PLACEHOLDER
Blob Container  : uploads
Cosmos Account  : PLACEHOLDER
Cosmos Conn     : PLACEHOLDER
Cosmos Database : ProductCatalogDb
Cosmos Container: Products
```

### Phase 3 — Azure Functions
```
Resource Group  : rg-learn-phase3
Storage Account : PLACEHOLDER
Function App    : PLACEHOLDER
Function URL    : https://PLACEHOLDER.azurewebsites.net
```

### Phase 4 — Messaging
```
Resource Group    : rg-learn-phase4
Service Bus NS    : PLACEHOLDER
Service Bus Conn  : PLACEHOLDER
Service Bus Queue : orders-queue
Event Grid Topic  : PLACEHOLDER
Event Grid Key    : PLACEHOLDER
Event Grid URL    : PLACEHOLDER
Event Hubs NS     : PLACEHOLDER
Event Hubs Conn   : PLACEHOLDER
Event Hub Name    : telemetry-hub
```

### Phase 5 — Security & Identity
```
Resource Group  : rg-learn-phase5
Key Vault Name  : PLACEHOLDER
Key Vault URL   : https://PLACEHOLDER.vault.azure.net/
AD App Name     : PLACEHOLDER
AD Client ID    : PLACEHOLDER
AD Tenant ID    : PLACEHOLDER (same as Your Azure Account above)
App Service Name: PLACEHOLDER
```

### Phase 6 — Monitoring
```
Resource Group      : rg-learn-phase6
Log Analytics WS    : law-learning
App Insights Name   : ai-taskmanager
App Insights Conn   : PLACEHOLDER
Action Group Name   : ag-learning-alerts
Alert Email         : PLACEHOLDER (your email)
```

### Phase 7 — Containers & AKS
```
Resource Group  : rg-learn-phase7
ACR Name        : PLACEHOLDER   (acrlearningXXXX — no hyphens)
ACI Name        : aci-taskmanager
AKS Cluster     : aks-learning
⚠️ Remember: DELETE AKS after 1-2 days to avoid charges!
```

### Phase 8 — DevOps & CI/CD
```
GitHub Repo     : PLACEHOLDER
Dev App Service : PLACEHOLDER
QA App Service  : PLACEHOLDER
Prod App Service: PLACEHOLDER
```

### Phase 9 — Capstone
```
Resource Group  : rg-learn-capstone
(All services above combined)
```

---

## Completed Phases

- [ ] Phase 1: App Service + Azure SQL
- [ ] Phase 2: Blob Storage + Cosmos DB
- [ ] Phase 3: Azure Functions
- [ ] Phase 4: Messaging
- [ ] Phase 5: Security & Identity
- [ ] Phase 6: Monitoring
- [ ] Phase 7: Containers + AKS
- [ ] Phase 8: DevOps + CI/CD
- [ ] Phase 9: Capstone

---

## My Notes

> Add your own notes, questions, and learnings here as you go.

### Phase 1 Notes
-

### Questions to Research
-

### Things That Tripped Me Up
-
