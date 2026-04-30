# Azure Free Tier Guide
### What's free, what's not, and how to avoid surprise charges

---

## Your Free Account — What You Get

### Always Free (No Expiry)

These are free forever, regardless of account age:

| Service | Free Limit | Notes |
|---------|-----------|-------|
| App Service | 10 apps, F1 tier | 60 CPU min/day, 1 GB storage |
| Azure Functions | 1 million executions/month | Consumption plan only |
| Azure SQL | 100,000 vCore-seconds/month | Serverless tier |
| Cosmos DB | 1,000 RU/s + 25 GB storage | Free account (1 per subscription) |
| Blob Storage | 5 GB LRS + 20K reads + 10K writes | Standard tier |
| Key Vault | 10,000 transactions/month | Standard tier |
| Event Grid | 100,000 operations/month | |
| Application Insights | 5 GB data/month | |
| Azure AD | Unlimited users | Basic features only |
| Service Bus | N/A — no permanent free tier | Basic = $0.05/million ops |

### 12-Month Free (From Account Creation Date)

| Service | Free Limit |
|---------|-----------|
| Azure SQL | 250 GB/month |
| Blob Storage | 5 GB additional |
| Virtual Machines | 750 hrs B1s Linux + 750 hrs B1s Windows |
| Event Hubs | 10M events/month (Basic) |

### $200 Credit — First 30 Days Only

Use this for services that aren't free tier:
- AKS cluster (~$1.50/day for 1 B2s node)
- Standard Service Bus topics
- Premium App Service
- Any VM beyond B1s

---

## Phase-by-Phase Cost Estimate

| Phase | Services | Est. Monthly Cost |
|-------|---------|------------------|
| Phase 1 | App Service F1 + SQL Serverless | $0–2 |
| Phase 2 | Blob Storage + Cosmos DB | $0 |
| Phase 3 | Functions Consumption | $0 |
| Phase 4 | Service Bus Basic + Event Grid + Event Hubs | $0–1 |
| Phase 5 | Key Vault + AD | $0 |
| Phase 6 | App Insights | $0 |
| Phase 7 | ACR Basic + ACI + AKS (2 days) | $5–15 |
| Phase 8 | No new services | $0 |
| Phase 9 | All above combined | $0–5 |
| **Total** | | **~$5–23** |

---

## Services That Will Drain Your Credit

### 🔴 AKS (Kubernetes) — ~$1.50/day per node
- **Rule:** Create → learn → delete within 48 hours
- Forgetting for a week = ~$10 wasted

### 🟡 Standard Service Bus — $10/month flat
- **Rule:** Use Basic tier for queues only
- Only upgrade to Standard if you specifically need Topics/Subscriptions
- Downgrade immediately after testing topics

### 🟡 App Service B1+ — $13–70/month
- **Rule:** Use F1 (free) for all learning
- F1 limitation: 60 CPU minutes/day — fine for demos
- Upgrade only when you need "Always On" or custom domains

### 🟡 Virtual Machines
- **Rule:** Stop VMs when not in use (Stopped ≠ Deallocated)
- Deallocated VMs don't charge for compute (only storage)
- Always **Stop and Deallocate**, not just Stop

---

## How to Check Your Current Spend

### In Azure Portal
```
portal.azure.com → Cost Management + Billing → Cost analysis
```
Set view to "Current month" and check daily.

### Via CLI
```bash
# Check current month estimated costs
az consumption usage list \
  --start-date $(date -d "$(date +%Y-%m-01)" +%Y-%m-%d) \
  --end-date $(date +%Y-%m-%d) \
  --output table
```

---

## Set a Budget Alert (Do This Once — Right Now)

```bash
az consumption budget create \
  --budget-name "azure-learning-budget" \
  --amount 20 \
  --time-grain Monthly \
  --category Cost
```

You'll get an email when spend approaches $20.

To add an email alert threshold:
1. portal.azure.com → Cost Management + Billing → Budgets
2. Click your budget → Add alert condition
3. Set: 50% = warning email, 90% = urgent email

---

## Emergency: How to Stop All Charges Immediately

If you think something is running up charges:

```bash
# List ALL resource groups with their resources
az group list --query "[].name" -o tsv

# Delete all learning resource groups at once
az group list \
  --query "[?starts_with(name,'rg-learn-')].name" \
  -o tsv | xargs -I{} az group delete --name {} --yes --no-wait

echo "All rg-learn-* groups are being deleted"
```

---

## Free Tier Gotchas

### Azure SQL Serverless
- Auto-pauses after 60 minutes of inactivity — good!
- First connection after pause takes 15–30 seconds — expected
- If your app has a health check pinging every minute, it never pauses — disable during off-hours

### App Service F1
- 60 CPU minutes per day — this is very little if your app is doing heavy work
- Shared infrastructure — performance varies
- No Always On — app sleeps after 20 min idle (cold start on next request)
- No deployment slots (need S1 for that)

### Cosmos DB Free Tier
- Only ONE free-tier Cosmos account per subscription
- If you already have one from another project, you can't get another free one
- Serverless is cheap but not the same as the free tier

### Functions Consumption
- Cold starts when function hasn't run in a while
- Not suitable for latency-sensitive production workloads
- Fine for learning and background tasks

---

## Quick Cleanup Commands Per Phase

```bash
# Phase 1
az group delete --name rg-learn-phase1 --yes --no-wait

# Phase 2
az group delete --name rg-learn-phase2 --yes --no-wait

# Phase 3
az group delete --name rg-learn-phase3 --yes --no-wait

# Phase 4
az group delete --name rg-learn-phase4 --yes --no-wait

# Phase 5 (also delete AD app)
az group delete --name rg-learn-phase5 --yes --no-wait
# az ad app delete --id YOUR-APP-CLIENT-ID

# Phase 6
az group delete --name rg-learn-phase6 --yes --no-wait

# Phase 7 (delete AKS FIRST)
az aks delete --name aks-learning --resource-group rg-learn-phase7 --yes --no-wait
az group delete --name rg-learn-phase7 --yes --no-wait

# Phase 8
az webapp delete --resource-group rg-learn-phase1 --name taskmanager-api-qa-YOURNAME
az webapp delete --resource-group rg-learn-phase1 --name taskmanager-api-prod-YOURNAME

# Phase 9
az group delete --name rg-learn-capstone --yes --no-wait

# Nuke everything at once (use with caution!)
az group list --query "[?starts_with(name,'rg-learn-')].name" -o tsv \
  | xargs -I{} az group delete --name {} --yes --no-wait
```
