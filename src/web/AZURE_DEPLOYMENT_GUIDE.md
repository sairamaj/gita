# Azure Static Web Apps Deployment Guide

## Overview

This guide provides step-by-step instructions to deploy the Gita Practice React application to Azure Static Web Apps.

## Prerequisites

- Azure account with active subscription
- GitHub account (for automated deployments)
- Application already built and tested locally
- Git installed on your machine

## Application Details

- **Type**: Static React/Vite application
- **Size**: ~159 MB (including audio files)
- **Build Command**: `npm run build`
- **Output Directory**: `dist`
- **App Location**: `src/web/client`

## Deployment Steps

### Step 1: Push Code to GitHub

If you haven't already pushed your code to GitHub:

```bash
# Initialize git repository (if not already done)
cd C:\sai\dev\gita
git add .
git commit -m "Prepare for Azure deployment"

# Create a new repository on GitHub, then:
git remote add origin https://github.com/<your-username>/gita-practice.git
git push -u origin main
```

### Step 2: Create Azure Static Web App

#### Option A: Via Azure Portal (Recommended for First Time)

1. **Sign in to Azure Portal**
   - Go to https://portal.azure.com
   - Sign in with your Azure account

2. **Create Static Web App Resource**
   - Click "Create a resource" (+ icon)
   - Search for "Static Web Apps"
   - Click "Create"

3. **Configure Basic Settings**
   - **Subscription**: Select your Azure subscription
   - **Resource Group**: Create new or select existing
     - Suggested name: `gita-practice-rg`
   - **Name**: `gita-practice-app` (must be globally unique)
   - **Plan type**: Select "Free" (sufficient for this app)
   - **Region**: Choose closest to your users
     - US: East US 2, West US 2
     - Europe: West Europe
     - Asia: East Asia, Southeast Asia

4. **Configure Deployment Details**
   - **Source**: Select "GitHub"
   - Click "Sign in with GitHub" and authorize Azure
   - **Organization**: Select your GitHub username/organization
   - **Repository**: Select your repository
   - **Branch**: Select "main" (or your default branch)

5. **Configure Build Details**
   - **Build Presets**: Select "Custom"
   - **App location**: `src/web/client/dist`
   - **Api location**: Leave empty (no API)
   - **Output location**: Leave empty
   - **App build command**: Leave empty (we build before deployment)

6. **Review and Create**
   - Click "Review + create"
   - Verify all settings
   - Click "Create"
   - Wait for deployment (2-3 minutes)

7. **Get Deployment Token**
   - Once created, go to your Static Web App resource
   - Navigate to "Overview"
   - Note the URL (e.g., `https://gita-practice-app.azurestaticapps.net`)
   - Go to "Configuration" → "Deployment token"
   - Copy the deployment token (you'll need this for GitHub Actions)

#### Option B: Via Azure CLI

```bash
# Login to Azure
az login

# Create resource group (if needed)
az group create --name gita-practice-rg --location eastus2

# Create Static Web App
az staticwebapp create \
  --name gita-practice-app \
  --resource-group gita-practice-rg \
  --source https://github.com/<your-username>/<your-repo> \
  --location "East US 2" \
  --branch main \
  --app-location "src/web/client/dist" \
  --output-location "" \
  --sku Free

# Get deployment token
az staticwebapp secrets list \
  --name gita-practice-app \
  --resource-group gita-practice-rg \
  --query "properties.apiKey" -o tsv
```

### Step 3: Configure GitHub Secrets

1. **Go to Your GitHub Repository**
   - Navigate to your repository on GitHub
   - Click "Settings" tab

2. **Add Azure Deployment Token**
   - In the left sidebar, click "Secrets and variables" → "Actions"
   - Click "New repository secret"
   - **Name**: `AZURE_STATIC_WEB_APPS_API_TOKEN`
   - **Value**: Paste the deployment token from Azure Portal
   - Click "Add secret"

### Step 4: Verify GitHub Actions Workflow

The workflow file has already been created at `.github/workflows/azure-static-web-apps.yml`.

Key features of the workflow:
- Triggers on push to `main` or `feature/spa` branches
- Builds the application using Node.js 18
- Deploys to Azure Static Web Apps
- Handles pull request previews

### Step 5: Deploy

#### Automatic Deployment (Recommended)

Simply push your code to GitHub:

```bash
cd C:\sai\dev\gita
git add .
git commit -m "Add Azure deployment configuration"
git push origin main
```

GitHub Actions will automatically:
1. Build your application
2. Deploy to Azure Static Web Apps
3. Provide the deployment URL

Monitor the deployment:
- Go to your GitHub repository
- Click "Actions" tab
- Watch the "Azure Static Web Apps CI/CD" workflow

#### Manual Deployment (Alternative)

If you prefer to deploy manually:

```bash
# Install Azure Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Navigate to client directory
cd C:\sai\dev\gita\src\web\client

# Build the application
npm run build

# Deploy (use the token from Azure Portal)
swa deploy ./dist --deployment-token <YOUR_DEPLOYMENT_TOKEN>
```

### Step 6: Verify Deployment

1. **Get Your App URL**
   - Go to Azure Portal → Your Static Web App
   - Copy the URL from "Overview" (e.g., `https://gita-practice-app.azurestaticapps.net`)

2. **Test the Application**
   - Open the URL in your browser
   - Verify the application loads
   - Test chapter selection
   - Test audio playback
   - Test Individual Practice mode
   - Test Group Practice mode
   - Check browser console for errors

3. **Test on Multiple Browsers**
   - Chrome
   - Firefox
   - Safari
   - Edge

4. **Test on Mobile** (if applicable)
   - Open on mobile device
   - Verify responsive design
   - Test audio playback

## Configuration Files

### staticwebapp.config.json

Located at `src/web/client/public/staticwebapp.config.json`, this file configures:

- **SPA Routing**: Redirects all routes to `index.html`
- **MIME Types**: Ensures `.m4a` files are served correctly
- **Cache Headers**: Optimizes caching for static assets

### GitHub Actions Workflow

Located at `.github/workflows/azure-static-web-apps.yml`, this file:

- Builds the application on every push
- Deploys to Azure automatically
- Creates preview environments for pull requests

## Custom Domain (Optional)

### Step 1: Add Custom Domain in Azure

1. Go to Azure Portal → Your Static Web App
2. Navigate to "Custom domains"
3. Click "Add"
4. Enter your domain name (e.g., `gita-practice.yourdomain.com`)
5. Choose validation method:
   - **TXT record** (recommended)
   - **CNAME record**

### Step 2: Configure DNS

Add the required DNS records at your domain registrar:

**For TXT validation:**
- Type: TXT
- Name: `@` or your subdomain
- Value: (provided by Azure)

**For CNAME:**
- Type: CNAME
- Name: your subdomain (e.g., `gita-practice`)
- Value: `<your-app>.azurestaticapps.net`

### Step 3: Verify and Enable

1. Wait for DNS propagation (5-60 minutes)
2. Click "Validate" in Azure Portal
3. Once validated, SSL certificate is automatically provisioned
4. Your app will be accessible at your custom domain

## Monitoring and Maintenance

### View Application Insights

1. Go to Azure Portal → Your Static Web App
2. Navigate to "Application Insights"
3. View metrics:
   - Page views
   - Load times
   - Errors
   - User sessions

### Monitor Bandwidth Usage

1. Go to Azure Portal → Your Static Web App
2. Navigate to "Metrics"
3. Monitor:
   - Data out (bandwidth)
   - Request count
   - Response time

### Set Up Alerts

1. Go to Azure Portal → Your Static Web App
2. Navigate to "Alerts"
3. Create alert rules for:
   - High bandwidth usage
   - Error rate
   - Downtime

## Troubleshooting

### Build Fails in GitHub Actions

**Issue**: Build fails with npm errors

**Solution**:
- Check Node.js version in workflow (should be 18+)
- Verify `package-lock.json` is committed
- Check build logs in GitHub Actions

### Audio Files Not Playing

**Issue**: Audio files return 404 or don't play

**Solution**:
- Verify `staticwebapp.config.json` is in `dist` folder
- Check MIME type configuration for `.m4a` files
- Verify audio files are in `dist/data/` folder

### Application Shows 404 on Refresh

**Issue**: Refreshing a route shows 404 error

**Solution**:
- Verify `navigationFallback` is configured in `staticwebapp.config.json`
- Ensure the config file is deployed to Azure

### Deployment Token Invalid

**Issue**: GitHub Actions fails with authentication error

**Solution**:
- Regenerate deployment token in Azure Portal
- Update `AZURE_STATIC_WEB_APPS_API_TOKEN` secret in GitHub

### App Exceeds Free Tier Limits

**Issue**: Warning about exceeding 250 MB limit

**Solution**:
- Current app is 159 MB (within limit)
- If needed, upgrade to Standard tier ($9/month)
- Go to Azure Portal → Your Static Web App → "Hosting plan" → "Change"

## Cost Management

### Free Tier Limits

- **Storage**: 250 MB (you're using 159 MB)
- **Bandwidth**: 100 GB/month
- **Custom domains**: 2
- **Staging environments**: 3

### Monitoring Costs

1. Go to Azure Portal → "Cost Management + Billing"
2. View your subscription costs
3. Set up budget alerts

### Optimizing Costs

- Use free tier for development/testing
- Monitor bandwidth usage
- Enable caching (already configured)
- Consider CDN for high-traffic scenarios

## Updating the Application

### Automatic Updates (via GitHub)

1. Make changes to your code
2. Commit and push to GitHub:
   ```bash
   git add .
   git commit -m "Update application"
   git push origin main
   ```
3. GitHub Actions automatically builds and deploys
4. Changes are live in 2-5 minutes

### Manual Updates

1. Build locally:
   ```bash
   cd src/web/client
   npm run build
   ```
2. Deploy using SWA CLI:
   ```bash
   swa deploy ./dist --deployment-token <YOUR_TOKEN>
   ```

## Rollback

### Via GitHub

1. Revert the problematic commit:
   ```bash
   git revert <commit-hash>
   git push origin main
   ```
2. GitHub Actions automatically deploys the reverted version

### Via Azure Portal

1. Go to Azure Portal → Your Static Web App
2. Navigate to "Environments"
3. Select previous successful deployment
4. Click "Promote to production"

## Security Best Practices

1. **Keep Secrets Secure**
   - Never commit deployment tokens to Git
   - Use GitHub Secrets for sensitive data

2. **Enable HTTPS**
   - Automatically enabled by Azure Static Web Apps
   - Custom domains get free SSL certificates

3. **Monitor Access**
   - Review application logs regularly
   - Set up alerts for unusual activity

4. **Keep Dependencies Updated**
   - Regularly update npm packages
   - Monitor security advisories

## Support and Resources

### Azure Documentation
- [Azure Static Web Apps Docs](https://docs.microsoft.com/azure/static-web-apps/)
- [Pricing Details](https://azure.microsoft.com/pricing/details/app-service/static/)

### GitHub Actions
- [GitHub Actions Docs](https://docs.github.com/actions)
- [Azure Static Web Apps Deploy Action](https://github.com/Azure/static-web-apps-deploy)

### Community Support
- [Azure Static Web Apps GitHub](https://github.com/Azure/static-web-apps)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-static-web-apps)

## Quick Reference

### Important URLs
- Azure Portal: https://portal.azure.com
- Your App URL: `https://<your-app-name>.azurestaticapps.net`
- GitHub Actions: `https://github.com/<username>/<repo>/actions`

### Key Commands
```bash
# Build application
cd src/web/client && npm run build

# Deploy manually
swa deploy ./dist --deployment-token <TOKEN>

# View Azure resources
az staticwebapp list --output table

# Get deployment token
az staticwebapp secrets list --name <app-name> --resource-group <rg-name>
```

### Configuration Files
- Workflow: `.github/workflows/azure-static-web-apps.yml`
- SWA Config: `src/web/client/public/staticwebapp.config.json`
- Build Config: `src/web/client/vite.config.js`

## Next Steps

1. ✅ Deploy to Azure Static Web Apps
2. ✅ Verify application works correctly
3. ⬜ Configure custom domain (optional)
4. ⬜ Set up monitoring and alerts
5. ⬜ Share URL with users
6. ⬜ Plan for scaling if needed

---

**Deployment Date**: _____________  
**Azure URL**: _____________  
**Custom Domain**: _____________  
**Deployed By**: _____________
