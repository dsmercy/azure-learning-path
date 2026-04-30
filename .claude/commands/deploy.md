# /deploy

Deploy the current phase project to Azure App Service.

## Usage
```
/deploy
/deploy <app-service-name>
```

## What This Command Does

1. Read active app name from `context/project-context.md`
2. Find the active phase `src/` folder
3. Run publish and deploy:

```bash
# Publish Release build
dotnet publish -c Release -o ./publish

# Zip output
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group <rg-from-context> \
  --name <app-name-from-context> \
  --src deploy.zip

# Smoke test (wait 15 seconds for startup)
sleep 15
curl -s https://<app-name>.azurewebsites.net/health
```

4. Print the live Swagger URL on success
5. On failure: show the last 50 lines of App Service logs

## Notes
- Always builds in **Release** mode for Azure deployment
- Cleans old `publish/` and `deploy.zip` before each run
- If health check fails, run: `az webapp log tail` to diagnose
