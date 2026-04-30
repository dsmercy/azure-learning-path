# Phase 7 — Azure Setup Guide
## Docker + Azure Container Registry + ACI + AKS

---

## ⚠️ Cost Warning — Read Before Starting

| Resource | Cost |
|----------|------|
| Azure Container Registry (Basic) | ~$5/month |
| Azure Container Instances | ~$1–3 while running |
| AKS Cluster (1 node, B2s VM) | **~$30–60/month** |

> **Strategy:** Create AKS → spend 1–2 days learning → DELETE IT.
> AKS at ~$1.50/day for 2 days = ~$3. That is fine for learning.
> If you forget to delete, it will drain your free credit quickly.
>
> Set a reminder right now to delete AKS after 48 hours.

---

## Prerequisites — Install Docker Desktop

Download and install: https://www.docker.com/products/docker-desktop

```bash
docker --version    # Verify install
docker ps           # Verify it's running
```

---

## What You Are Creating

```
Resource Group: rg-learn-phase7
├── Azure Container Registry: acrlearningXXXX  (stores your images)
├── Azure Container Instances: aci-taskmanager  (single container, quick deploy)
└── AKS Cluster: aks-learning                   (1 node Kubernetes — delete after learning!)
```

---

## Step 1 — Create Resource Group

```bash
az group create --name rg-learn-phase7 --location eastus
```

---

## Step 2 — Create Azure Container Registry

ACR names: lowercase letters and numbers only, 5–50 chars, globally unique.

```bash
az acr create \
  --name acrlearningYOURNAME \
  --resource-group rg-learn-phase7 \
  --sku Basic \
  --admin-enabled true
```

---

## Step 3 — Get ACR Credentials

```bash
ACR_USERNAME=$(az acr credential show \
  --name acrlearningYOURNAME \
  --query username -o tsv)

ACR_PASSWORD=$(az acr credential show \
  --name acrlearningYOURNAME \
  --query "passwords[0].value" -o tsv)

echo "ACR Login Server: acrlearningYOURNAME.azurecr.io"
echo "Username: $ACR_USERNAME"
echo "Password: $ACR_PASSWORD"
```

**Copy all three to `context/project-context.md`.**

---

## Step 4 — Explore ACR in Azure Portal

1. portal.azure.com → Container registries → your registry
2. Repositories → empty for now (no images pushed yet)
3. Access keys → see admin username and passwords

---

## Step 5 — Create AKS Cluster (⚠️ Costs Money — Delete After Learning)

```bash
az aks create \
  --name aks-learning \
  --resource-group rg-learn-phase7 \
  --node-count 1 \
  --node-vm-size Standard_B2s \
  --attach-acr acrlearningYOURNAME \
  --generate-ssh-keys
```

> ⏳ This takes **5–10 minutes**. The `--attach-acr` flag lets your AKS cluster pull images from your ACR without extra auth.

---

## Step 6 — Connect kubectl to Your Cluster

```bash
# Download cluster credentials into your local kubeconfig
az aks get-credentials \
  --resource-group rg-learn-phase7 \
  --name aks-learning

# Verify
kubectl get nodes
# Should show: aks-nodepool1-xxxxx   Ready   agent   Xm   v1.29.x
```

---

## Step 7 — Install kubectl (if not installed)

```bash
# Windows (winget)
winget install Kubernetes.kubectl

# macOS
brew install kubectl

# Or via Azure CLI
az aks install-cli
```

---

## Core Kubernetes Concepts

**Objects you'll work with:**

| Object | What It Does |
|--------|-------------|
| **Pod** | Smallest unit — one or more containers running together |
| **Deployment** | Manages a set of identical pods (handles restarts, rolling updates) |
| **Service** | Gives your pods a stable network endpoint (like a load balancer) |
| **Ingress** | HTTP routing rules (routes paths/hostnames to services) |
| **ConfigMap** | Config data (non-sensitive) stored in Kubernetes |
| **Secret** | Sensitive data stored in Kubernetes (use Key Vault in production) |

**Essential kubectl commands:**
```bash
kubectl get pods                     # List running pods
kubectl get services                 # List services
kubectl get deployments              # List deployments
kubectl describe pod POD-NAME        # Detailed info about a pod
kubectl logs POD-NAME                # View pod logs
kubectl logs -f POD-NAME             # Follow logs (like tail -f)
kubectl exec -it POD-NAME -- bash    # Open shell inside pod
kubectl delete pod POD-NAME          # Delete a pod (Deployment recreates it)
kubectl scale deployment NAME --replicas=3   # Scale to 3 pods
kubectl rollout status deployment NAME       # Watch rolling update progress
kubectl rollout undo deployment NAME         # Rollback to previous version
```

---

## Step 8 — Explore AKS in Azure Portal

1. portal.azure.com → Kubernetes services → aks-learning
2. Workloads → see deployments (empty until you deploy)
3. Services and ingresses → see the Kubernetes default services
4. Monitoring → Insights → requires Container Insights to be enabled

---

## ✅ Phase 7 Azure Setup Checklist

- [ ] Docker Desktop installed and running
- [ ] Resource group `rg-learn-phase7` created
- [ ] ACR created
- [ ] ACR login server, username, password saved
- [ ] AKS cluster created (1 node)
- [ ] kubectl configured: `kubectl get nodes` shows node as Ready
- [ ] ⏰ Calendar reminder set to delete AKS within 48 hours!

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 7 — Dockerize the Task Manager API, push to ACR, deploy to ACI and AKS
```

---

## Cleanup (IMPORTANT — Delete AKS First!)

```bash
# Delete AKS first (most expensive)
az aks delete --name aks-learning --resource-group rg-learn-phase7 --yes --no-wait

# Then delete everything else
az group delete --name rg-learn-phase7 --yes --no-wait
```
