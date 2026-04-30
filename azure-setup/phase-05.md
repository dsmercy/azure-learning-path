# Phase 5 — Azure Setup Guide
## Azure AD + RBAC + Managed Identity + Key Vault

---

## What You Are Creating

```
Resource Group: rg-learn-phase5
├── Key Vault: kv-learning-XXXX           (stores all secrets)
├── App Service: app-secured-XXXX         (your API, with Managed Identity ON)
│   └── System-Assigned Managed Identity  (gets automatic Azure AD identity)
└── Azure AD App Registration             (defines the API for authentication)
```

**Estimated cost:** $0/month — all free tier.

---

## Step 1 — Create Resource Group and App Service

```bash
az group create --name rg-learn-phase5 --location eastus

az appservice plan create \
  --name plan-phase5 \
  --resource-group rg-learn-phase5 \
  --sku F1 --is-linux

az webapp create \
  --name app-secured-YOURNAME \
  --resource-group rg-learn-phase5 \
  --plan plan-phase5 \
  --runtime "DOTNETCORE:8.0"
```

---

## Step 2 — Create Key Vault

```bash
az keyvault create \
  --name kv-learning-YOURNAME \
  --resource-group rg-learn-phase5 \
  --location eastus \
  --enable-rbac-authorization true
```

**`--enable-rbac-authorization true`** = use Azure RBAC to control access (modern approach).
The older approach used "Access Policies" — RBAC is preferred now.

---

## Step 3 — Grant Yourself Access to Key Vault

You need permission to add secrets to your own vault.

```bash
# Get your own Object ID in Azure AD
MY_OID=$(az ad signed-in-user show --query id -o tsv)
echo "My Object ID: $MY_OID"

# Get Key Vault resource ID
KV_ID=$(az keyvault show --name kv-learning-YOURNAME --query id -o tsv)

# Assign Key Vault Secrets Officer role to yourself
az role assignment create \
  --assignee $MY_OID \
  --role "Key Vault Secrets Officer" \
  --scope $KV_ID

echo "You can now add/read/delete secrets"
```

**RBAC concept:**
- A **role** = a set of permissions (what can be done)
- A **role assignment** = who gets that role, on what scope
- **Scope** can be: management group → subscription → resource group → individual resource
- Narrower scope = less privilege = better security

---

## Step 4 — Add Secrets to Key Vault

```bash
# Store a fake database connection string
az keyvault secret set \
  --vault-name kv-learning-YOURNAME \
  --name "SqlConnectionString" \
  --value "Server=tcp:fake-server.database.windows.net;..."

# Store a fake external API key
az keyvault secret set \
  --vault-name kv-learning-YOURNAME \
  --name "ExternalApiKey" \
  --value "sk-fake-key-12345abcde"

# Store a JWT signing key
az keyvault secret set \
  --vault-name kv-learning-YOURNAME \
  --name "JwtSigningKey" \
  --value "my-super-secret-jwt-key-change-in-production"

# List secret names (values are hidden)
az keyvault secret list --vault-name kv-learning-YOURNAME --query "[].name" -o table
```

**In Azure Portal:**
1. Go to Key Vault → Secrets
2. You see the secret names — but clicking one shows only metadata
3. To see the value, click the secret → click the version → Show Secret Value
4. Every time you update a secret, a new **version** is created — old versions are kept

---

## Step 5 — Enable Managed Identity on App Service

```bash
az webapp identity assign \
  --name app-secured-YOURNAME \
  --resource-group rg-learn-phase5
```

This command:
1. Creates an identity for your App Service in Azure AD
2. Returns an Object ID (the identity's ID)
3. Your app now has an identity — like a service account — but with no password to manage

```bash
# Get the Managed Identity Object ID
MI_OID=$(az webapp identity show \
  --name app-secured-YOURNAME \
  --resource-group rg-learn-phase5 \
  --query principalId -o tsv)

echo "Managed Identity Object ID: $MI_OID"
```

**Copy to `context/project-context.md`.**

---

## Step 6 — Grant Managed Identity Access to Key Vault

```bash
az role assignment create \
  --assignee $MI_OID \
  --role "Key Vault Secrets User" \
  --scope $KV_ID
```

**What just happened:**
- Your App Service now has permission to **read** Key Vault secrets
- It does this using its Managed Identity — **no password, no connection string**
- In code: `new DefaultAzureCredential()` automatically uses the Managed Identity when running in Azure
- When running locally, it uses your `az login` credentials instead

This is why Managed Identities are powerful: **zero secrets to manage**.

---

## Step 7 — Register Your API in Azure AD

This lets you protect API endpoints with Azure AD authentication.

```bash
# Register the application
APP_CLIENT_ID=$(az ad app create \
  --display-name "secured-api-demo" \
  --query appId -o tsv)

echo "Client ID (App ID): $APP_CLIENT_ID"

# Create a service principal (needed for authentication to work)
az ad sp create --id $APP_CLIENT_ID

# Get Tenant ID
TENANT_ID=$(az account show --query tenantId -o tsv)
echo "Tenant ID: $TENANT_ID"
```

**Copy both to `context/project-context.md`.**

**Concepts:**
- **App Registration** = defines your application to Azure AD
- **Service Principal** = the runtime instance of the registration
- **Client ID** = unique identifier for your app (used in appsettings.json)
- **Tenant ID** = your organization's Azure AD directory

---

## Step 8 — Set Key Vault URL on App Service

```bash
az webapp config appsettings set \
  --name app-secured-YOURNAME \
  --resource-group rg-learn-phase5 \
  --settings KeyVaultUrl="https://kv-learning-YOURNAME.vault.azure.net/"
```

Your deployed API will read this setting and load all Key Vault secrets into its configuration automatically.

---

## Step 9 — Explore in Azure Portal

**Key Vault:**
1. portal.azure.com → Key vaults → your vault
2. Secrets → see all three secrets
3. Access control (IAM) → Role assignments → see yourself and the Managed Identity

**App Service:**
1. portal.azure.com → App Service → your app
2. Identity → System assigned → Status = **On** → Object ID shown
3. Configuration → Application settings → see `KeyVaultUrl`

**Azure AD:**
1. portal.azure.com → Microsoft Entra ID (Azure Active Directory)
2. App registrations → secured-api-demo
3. Overview → Client ID, Tenant ID

---

## ✅ Phase 5 Azure Setup Checklist

- [ ] Resource group `rg-learn-phase5` created
- [ ] App Service created
- [ ] Key Vault created (RBAC mode)
- [ ] Granted yourself Secrets Officer on Key Vault
- [ ] Added 3 test secrets to Key Vault
- [ ] Enabled Managed Identity on App Service
- [ ] Managed Identity Object ID saved
- [ ] Granted Managed Identity "Key Vault Secrets User" role
- [ ] Registered API app in Azure AD
- [ ] Client ID and Tenant ID saved
- [ ] KeyVaultUrl set in App Service config

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 5 — create the Secured API with Azure AD auth and Key Vault integration
```

---

## Cleanup

```bash
az group delete --name rg-learn-phase5 --yes --no-wait

# Also delete the AD App Registration
az ad app delete --id $APP_CLIENT_ID
```
