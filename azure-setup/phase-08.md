# Phase 8 — Azure Setup Guide
## GitHub Actions + Azure DevOps — CI/CD Pipelines

---

## What You Are Setting Up

No new Azure infrastructure is needed — you reuse the Phase 1 App Service.

What you are configuring:
- **GitHub repository** with branch protection
- **GitHub Environments**: Development, QA, Production (with approval gate)
- **GitHub Secrets**: publish profiles for each environment
- **GitHub Actions workflow**: build → test → deploy pipeline
- **Azure DevOps** (optional): explore as an alternative

**Estimated cost:** $0 — GitHub Actions gives 2,000 free minutes/month.

---

## Step 1 — Create a GitHub Repository

1. Go to https://github.com/new
2. Name it: `azure-dotnet-learning`
3. Set it to **Private**
4. Do NOT initialize with README (you already have files)
5. Click **Create repository**

---

## Step 2 — Push Your Phase 1 Project to GitHub

```bash
cd Phase-01-AppService-AzureSQL/src/TaskManagerApi

git init
git add .
git commit -m "feat(phase1): initial Task Manager API"

# Connect to your GitHub repo
git remote add origin https://github.com/YOUR-USERNAME/azure-dotnet-learning.git
git branch -M main
git push -u origin main
```

---

## Step 3 — Create App Services for Three Environments

You need separate App Services for Dev, QA, and Prod.

```bash
# Reuse the Phase 1 plan (or create a new one)
# Dev (reuse Phase 1 App Service)
DEV_APP="taskmanager-api-YOURNAME"   # From Phase 1

# QA
az webapp create \
  --name taskmanager-api-qa-YOURNAME \
  --resource-group rg-learn-phase1 \
  --plan plan-taskmanager-dev \
  --runtime "DOTNETCORE:8.0"

# Prod
az webapp create \
  --name taskmanager-api-prod-YOURNAME \
  --resource-group rg-learn-phase1 \
  --plan plan-taskmanager-dev \
  --runtime "DOTNETCORE:8.0"
```

**Copy all three app names to `context/project-context.md`.**

---

## Step 4 — Get Publish Profiles for Each App

A publish profile is an XML file containing deployment credentials.

```bash
# Dev
az webapp deployment list-publishing-profiles \
  --resource-group rg-learn-phase1 \
  --name taskmanager-api-YOURNAME \
  --xml

# QA
az webapp deployment list-publishing-profiles \
  --resource-group rg-learn-phase1 \
  --name taskmanager-api-qa-YOURNAME \
  --xml

# Prod
az webapp deployment list-publishing-profiles \
  --resource-group rg-learn-phase1 \
  --name taskmanager-api-prod-YOURNAME \
  --xml
```

Each command outputs XML. Copy each one — you'll add them as GitHub Secrets.

---

## Step 5 — Add GitHub Secrets

1. Go to your GitHub repo → **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret** for each:

| Secret Name | Value |
|-------------|-------|
| `AZURE_WEBAPP_PUBLISH_PROFILE_DEV`  | XML from Dev app |
| `AZURE_WEBAPP_PUBLISH_PROFILE_QA`   | XML from QA app |
| `AZURE_WEBAPP_PUBLISH_PROFILE_PROD` | XML from Prod app |

---

## Step 6 — Create GitHub Environments

Environments add deployment protection rules — most importantly, **manual approval for Production**.

1. Go to GitHub repo → **Settings** → **Environments**

2. Create **Development** environment:
   - No protection rules
   - Add variable: `AZURE_WEBAPP_NAME` = `taskmanager-api-YOURNAME`

3. Create **QA** environment:
   - No protection rules
   - Add variable: `AZURE_WEBAPP_NAME` = `taskmanager-api-qa-YOURNAME`

4. Create **Production** environment:
   - ✅ Check **Required reviewers** → add yourself
   - ✅ Check **Wait timer** → set to 5 minutes (optional buffer)
   - Add variable: `AZURE_WEBAPP_NAME` = `taskmanager-api-prod-YOURNAME`

> Now when the pipeline reaches Production, it pauses and sends you an email asking: **"Approve this deployment?"**

---

## Step 7 — Understanding the Pipeline Flow

```
git push to main
       │
       ▼
  ┌─────────┐
  │  Build  │  dotnet build + test
  └────┬────┘
       │ ✅ build passes
       ▼
  ┌─────────┐
  │   Dev   │  auto-deploys immediately
  └────┬────┘
       │ ✅ smoke test passes
       ▼
  ┌─────────┐
  │   QA    │  auto-deploys after Dev succeeds
  └────┬────┘
       │ ✅ integration tests pass
       ▼
  ┌─────────┐
  │  Prod   │  ⏸ PAUSES — waits for your approval in GitHub
  └─────────┘
```

---

## Step 8 — Set Up Branch Protection (Optional but Good Practice)

1. GitHub repo → **Settings** → **Branches**
2. Click **Add branch protection rule**
3. Branch name pattern: `main`
4. Enable:
   - ✅ Require a pull request before merging
   - ✅ Require status checks to pass (add your build check once pipeline runs once)

---

## Azure DevOps (Optional — Explore as Alternative)

If you want to explore Azure DevOps as well:

1. Go to https://dev.azure.com and sign in with your Azure account
2. Create a new organization and project
3. Pipelines → New Pipeline → GitHub → select your repo → choose "Starter Pipeline"
4. Azure DevOps has two pipeline types:
   - **Classic (GUI)** → visual drag-and-drop, good for learning
   - **YAML** → code-based, industry standard
5. Environments in Azure DevOps:
   - Pipelines → Environments → New Environment
   - Add **Approvals and Checks** to your Production environment

---

## ✅ Phase 8 Setup Checklist

- [ ] GitHub repository created and code pushed
- [ ] Dev, QA, Prod App Services created
- [ ] Publish profiles downloaded for all three environments
- [ ] GitHub Secrets added (3 publish profiles)
- [ ] GitHub Environments created (Development, QA, Production)
- [ ] Production environment has required reviewer (you)
- [ ] App names saved in `context/project-context.md`

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 8 — create the GitHub Actions workflow for Dev/QA/Prod deployment
```

---

## Cleanup

```bash
# Delete QA and Prod apps (Dev is Phase 1's app — keep if still learning)
az webapp delete --resource-group rg-learn-phase1 --name taskmanager-api-qa-YOURNAME
az webapp delete --resource-group rg-learn-phase1 --name taskmanager-api-prod-YOURNAME
```
