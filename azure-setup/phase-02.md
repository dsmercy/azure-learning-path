# Phase 2 — Azure Setup Guide
## Blob Storage + Azure Cosmos DB

---

## What You Are Creating

```
Resource Group: rg-learn-phase2
├── Storage Account: stlearningXXXX
│   └── Blob Container: uploads  (private)
└── Cosmos DB Account: cosmos-learning-XXXX  (Serverless)
    └── Database: ProductCatalogDb
        └── Container: Products  (partition key: /category)
```

**Estimated cost:** $0/month — both services have permanent free tiers at learning scale.

---

## Step 1 — Create Resource Group

```bash
az group create --name rg-learn-phase2 --location eastus
```

---

## Step 2 — Create Storage Account

Storage account names must be **globally unique**, **3–24 characters**, **lowercase letters and numbers only** (no hyphens).

```bash
# Pick a unique name: st + your initials + random number
# Example: stlearningjd4829

az storage account create \
  --name stlearningYOURINITIALS001 \
  --resource-group rg-learn-phase2 \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2 \
  --access-tier Hot
```

**Tier explanation:**
- `Hot` → frequently accessed data (your uploaded files). Slightly higher storage cost, lower access cost.
- `Cool` → infrequent access (30+ day old archives). Lower storage cost, higher access cost.
- `Archive` → rarely accessed backups.

---

## Step 3 — Get Storage Account Key and Connection String

```bash
# Get the account key
STORAGE_KEY=$(az storage account keys list \
  --resource-group rg-learn-phase2 \
  --account-name stlearningYOURINITIALS001 \
  --query "[0].value" -o tsv)

echo "Key: $STORAGE_KEY"

# Get connection string
az storage account show-connection-string \
  --resource-group rg-learn-phase2 \
  --name stlearningYOURINITIALS001 \
  -o tsv
```

**Copy the connection string to `context/project-context.md`.**

---

## Step 4 — Create Blob Container

```bash
az storage container create \
  --name uploads \
  --account-name stlearningYOURINITIALS001 \
  --account-key $STORAGE_KEY \
  --public-access off
```

**`--public-access off`** means blobs are private. To download, you'll need either:
- The account key (server-side only)
- A SAS token (time-limited signed URL — what you'll learn in Phase 2)

---

## Step 5 — Explore Storage in Azure Portal

1. Go to portal.azure.com → Storage accounts → your account
2. Click **Containers** → you see `uploads`
3. Click `uploads` → it's empty for now
4. Come back here after you deploy the API and upload a file

---

## Step 6 — Create Cosmos DB Account (Serverless)

Cosmos DB account names must be **globally unique** and **lowercase**.

```bash
az cosmosdb create \
  --name cosmos-learning-YOURNAME \
  --resource-group rg-learn-phase2 \
  --kind GlobalDocumentDB \
  --locations regionName=eastus failoverPriority=0 isZoneRedundant=False \
  --capabilities EnableServerless
```

> ⏳ This takes **3–5 minutes**. Wait for it to complete.

**Serverless Cosmos DB:**
- Free for first 1,000 RU/s + 25 GB — plenty for learning
- Charges per Request Unit (RU) consumed — think of it as compute per operation
- A simple point-read = ~1 RU. A cross-partition query = many more RUs.

---

## Step 7 — Create Cosmos Database and Container

```bash
# Create the database
az cosmosdb sql database create \
  --account-name cosmos-learning-YOURNAME \
  --resource-group rg-learn-phase2 \
  --name ProductCatalogDb

# Create the container with a partition key
az cosmosdb sql container create \
  --account-name cosmos-learning-YOURNAME \
  --resource-group rg-learn-phase2 \
  --database-name ProductCatalogDb \
  --name Products \
  --partition-key-path "/category"
```

**What is a partition key?**
Cosmos DB splits data across physical storage partitions. The partition key decides which partition a document goes to.
- Queries that include the partition key are **fast and cheap** (one partition)
- Queries without it scan **all partitions** (slow and expensive)
- Choose a field with many distinct values that appears in most queries
- `/category` is good for a product catalog: Electronics, Clothing, Books, etc.

---

## Step 8 — Get Cosmos DB Connection String

```bash
az cosmosdb keys list \
  --name cosmos-learning-YOURNAME \
  --resource-group rg-learn-phase2 \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" -o tsv
```

**Copy to `context/project-context.md`.**

---

## Step 9 — Explore Cosmos DB in Azure Portal

1. Go to portal.azure.com → Azure Cosmos DB → your account
2. Click **Data Explorer**
3. Expand: `ProductCatalogDb` → `Products`
4. It's empty now. After you add items via the API, come back and explore.
5. Try the **New SQL Query** button — you can query documents here

---

## ✅ Phase 2 Azure Setup Checklist

- [ ] Resource group `rg-learn-phase2` created
- [ ] Storage account created
- [ ] Storage connection string saved to `context/project-context.md`
- [ ] Blob container `uploads` created (private)
- [ ] Cosmos DB account created (Serverless)
- [ ] Database `ProductCatalogDb` created
- [ ] Container `Products` created with `/category` partition key
- [ ] Cosmos connection string saved to `context/project-context.md`

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 2 — create the File Upload API and Product Catalog API
```

---

## Cleanup

```bash
az group delete --name rg-learn-phase2 --yes --no-wait
```
