# /cleanup

Delete Azure resources for a completed phase.

## Usage
```
/cleanup           # Delete active phase resources
/cleanup all       # Delete all rg-learn-* resource groups
```

## What This Command Does

1. Read resource group name from `context/project-context.md`
2. Show a list of resources that will be deleted
3. Ask for confirmation
4. Run deletion:

```bash
# Show what will be deleted
az resource list --resource-group <rg-name> --output table

# Delete
az group delete --name <rg-name> --yes --no-wait
echo "Deletion started — takes 3-5 minutes in background"
```

5. Clear the "Active Resources" section in `context/project-context.md`
6. Mark phase as ✅ Done in CLAUDE.md phase table

## Special Cases

### Phase 7 (AKS) — delete cluster FIRST
```bash
az aks delete --name <aks-name> --resource-group <rg-name> --yes --no-wait
# Wait 2 minutes, then delete the resource group
az group delete --name <rg-name> --yes --no-wait
```

### Delete ALL learning resources at once
```bash
az group list \
  --query "[?starts_with(name,'rg-learn-')].name" \
  -o tsv | xargs -I{} az group delete --name {} --yes --no-wait
```

## Notes
- Always confirm before deleting — there's no undo
- F1 App Service is free, so only SQL/Cosmos/AKS need urgent cleanup
- After cleanup, update `context/project-context.md`
