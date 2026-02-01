# Azure Deployment - Command Reference

Quick reference for all commands needed to deploy to Azure Static Web Apps.

## Local Development

### Install Dependencies
```bash
cd C:\sai\dev\gita\src\web\client
npm install
```

### Start Dev Server
```bash
npm run dev
# Opens at http://localhost:5173
```

### Build for Production
```bash
npm run build
# Output: dist/ folder
```

### Preview Production Build
```bash
npm run preview
# Opens at http://localhost:4173
```

### Check Build Size
```powershell
(Get-ChildItem -Path "C:\sai\dev\gita\src\web\client\dist" -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
# Should show ~159 MB
```

## Git Commands

### Initial Setup
```bash
cd C:\sai\dev\gita
git init
git add .
git commit -m "Initial commit with Azure deployment"
```

### Push to GitHub
```bash
# Add remote (first time only)
git remote add origin https://github.com/<username>/<repo>.git

# Push to GitHub
git push -u origin main
```

### Update and Deploy
```bash
# Make changes, then:
git add .
git commit -m "Your commit message"
git push origin main
# GitHub Actions automatically deploys
```

### Check Git Status
```bash
git status
git log --oneline -5
```

## Azure CLI Commands

### Login to Azure
```bash
az login
```

### Create Resource Group
```bash
az group create \
  --name gita-practice-rg \
  --location eastus2
```

### Create Static Web App
```bash
az staticwebapp create \
  --name gita-practice-app \
  --resource-group gita-practice-rg \
  --source https://github.com/<username>/<repo> \
  --location "East US 2" \
  --branch main \
  --app-location "src/web/client/dist" \
  --output-location "" \
  --sku Free
```

### List Static Web Apps
```bash
az staticwebapp list --output table
```

### Get Deployment Token
```bash
az staticwebapp secrets list \
  --name gita-practice-app \
  --resource-group gita-practice-rg \
  --query "properties.apiKey" -o tsv
```

### Show App Details
```bash
az staticwebapp show \
  --name gita-practice-app \
  --resource-group gita-practice-rg
```

### Delete Static Web App (if needed)
```bash
az staticwebapp delete \
  --name gita-practice-app \
  --resource-group gita-practice-rg
```

## Azure Static Web Apps CLI

### Install SWA CLI
```bash
npm install -g @azure/static-web-apps-cli
```

### Deploy Manually
```bash
cd C:\sai\dev\gita\src\web\client
npm run build
swa deploy ./dist --deployment-token <YOUR_TOKEN>
```

### Start Local SWA Emulator
```bash
swa start ./dist --port 4280
```

### Login to Azure (SWA CLI)
```bash
swa login
```

## GitHub CLI Commands

### Install GitHub CLI
```bash
# Windows (using winget)
winget install --id GitHub.cli

# Or download from https://cli.github.com/
```

### Login to GitHub
```bash
gh auth login
```

### Create Repository
```bash
gh repo create gita-practice --public --source=. --remote=origin
```

### Add Secret
```bash
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN
# Paste token when prompted
```

### View Secrets
```bash
gh secret list
```

### View Workflow Runs
```bash
gh run list
```

### Watch Workflow
```bash
gh run watch
```

## Verification Commands

### Check if staticwebapp.config.json Exists
```powershell
Test-Path "C:\sai\dev\gita\src\web\client\public\staticwebapp.config.json"
# Should return: True
```

### Check if Workflow File Exists
```powershell
Test-Path "C:\sai\dev\gita\.github\workflows\azure-static-web-apps.yml"
# Should return: True
```

### List Files in Dist
```powershell
Get-ChildItem -Path "C:\sai\dev\gita\src\web\client\dist" -Recurse | Select-Object FullName
```

### Count Files in Dist
```powershell
(Get-ChildItem -Path "C:\sai\dev\gita\src\web\client\dist" -Recurse -File).Count
```

## Troubleshooting Commands

### Clear Node Modules and Reinstall
```bash
cd C:\sai\dev\gita\src\web\client
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json
npm install
```

### Clear Build Cache
```bash
Remove-Item -Recurse -Force dist
npm run build
```

### Check Node and npm Versions
```bash
node --version  # Should be 14+
npm --version
```

### View GitHub Actions Logs
```bash
gh run view --log
```

### Test Local Build
```bash
npm run build
npm run preview
# Open http://localhost:4173
```

## Monitoring Commands

### View Azure Metrics
```bash
az monitor metrics list \
  --resource /subscriptions/<sub-id>/resourceGroups/gita-practice-rg/providers/Microsoft.Web/staticSites/gita-practice-app \
  --metric "DataOut"
```

### View Recent Deployments
```bash
gh run list --workflow=azure-static-web-apps.yml --limit 5
```

## Quick Deploy Sequence

### First Time Deployment
```bash
# 1. Build
cd C:\sai\dev\gita\src\web\client
npm run build

# 2. Commit
cd C:\sai\dev\gita
git add .
git commit -m "Add Azure deployment configuration"

# 3. Push (triggers automatic deployment)
git push origin main

# 4. Monitor
gh run watch
```

### Subsequent Deployments
```bash
# Make changes, then:
cd C:\sai\dev\gita
git add .
git commit -m "Update application"
git push origin main
```

## Environment Variables

### Set for Development
```bash
# Create .env.local in src/web/client/
echo "VITE_API_BASE=http://localhost:5173" > .env.local
```

### View Environment
```bash
# In Azure Portal → Configuration → Application settings
# Or via CLI:
az staticwebapp appsettings list \
  --name gita-practice-app \
  --resource-group gita-practice-rg
```

## URLs and Resources

### Important URLs
- Azure Portal: https://portal.azure.com
- GitHub: https://github.com
- Your App: https://gita-practice-app.azurestaticapps.net
- GitHub Actions: https://github.com/<username>/<repo>/actions

### Documentation
- Azure SWA Docs: https://docs.microsoft.com/azure/static-web-apps/
- GitHub Actions: https://docs.github.com/actions
- Vite Docs: https://vitejs.dev/

## Common Workflows

### Update Application
```bash
# 1. Make changes
# 2. Test locally
npm run dev

# 3. Build and test
npm run build
npm run preview

# 4. Deploy
git add .
git commit -m "Your changes"
git push origin main
```

### Rollback to Previous Version
```bash
# Find previous commit
git log --oneline -5

# Revert to previous commit
git revert <commit-hash>
git push origin main
```

### Create Feature Branch
```bash
git checkout -b feature/new-feature
# Make changes
git add .
git commit -m "New feature"
git push origin feature/new-feature
# Creates preview environment automatically
```

## Cleanup Commands

### Remove Local Build
```bash
Remove-Item -Recurse -Force C:\sai\dev\gita\src\web\client\dist
```

### Remove Node Modules
```bash
Remove-Item -Recurse -Force C:\sai\dev\gita\src\web\client\node_modules
```

### Delete Azure Resources (if needed)
```bash
# Delete Static Web App
az staticwebapp delete \
  --name gita-practice-app \
  --resource-group gita-practice-rg

# Delete Resource Group (deletes everything)
az group delete --name gita-practice-rg
```

## Tips

### Faster npm install
```bash
npm ci  # Uses package-lock.json, faster than npm install
```

### Watch Build Output
```bash
npm run build -- --watch
```

### Check for Updates
```bash
npm outdated
npm update
```

### Clear npm Cache
```bash
npm cache clean --force
```

---

**Quick Reference Card**

| Task | Command |
|------|---------|
| Build | `npm run build` |
| Deploy | `git push origin main` |
| View logs | `gh run view --log` |
| Check status | `gh run list` |
| Local dev | `npm run dev` |
| Preview build | `npm run preview` |
