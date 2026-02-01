# Azure Deployment - Quick Start

## 🚀 Deploy in 5 Minutes

### Prerequisites
- Azure account
- GitHub account
- Code pushed to GitHub

### Step 1: Create Azure Static Web App (2 min)

1. Go to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → Search "Static Web Apps" → Create
3. Configure:
   - **Resource Group**: Create new (e.g., `gita-practice-rg`)
   - **Name**: `gita-practice-app` (must be unique)
   - **Plan**: Free
   - **Region**: Choose closest to you
   - **Source**: GitHub
   - **Repository**: Select your repo
   - **Branch**: main
   - **App location**: `src/web/client/dist`
   - **Output location**: (leave empty)
4. Click "Review + create" → "Create"

### Step 2: Add GitHub Secret (1 min)

1. In Azure Portal, go to your Static Web App → "Configuration" → "Deployment token"
2. Copy the token
3. Go to GitHub repo → Settings → Secrets and variables → Actions
4. Click "New repository secret"
   - Name: `AZURE_STATIC_WEB_APPS_API_TOKEN`
   - Value: (paste token)
5. Click "Add secret"

### Step 3: Deploy (2 min)

```bash
git add .
git commit -m "Deploy to Azure"
git push origin main
```

Watch deployment at: `https://github.com/<username>/<repo>/actions`

### Step 4: Access Your App

Your app will be live at: `https://gita-practice-app.azurestaticapps.net`

## ✅ Verification Checklist

- [ ] App loads without errors
- [ ] Chapter selection works
- [ ] Audio plays correctly
- [ ] Individual practice works
- [ ] Group practice works

## 📊 What You Get (Free Tier)

- ✅ 250 MB storage (you use 159 MB)
- ✅ 100 GB bandwidth/month
- ✅ Global CDN
- ✅ Free SSL certificate
- ✅ Custom domains (2)
- ✅ Automatic deployments
- ✅ Staging environments (3)

## 🔧 Troubleshooting

### Build Fails
Check GitHub Actions logs: `https://github.com/<username>/<repo>/actions`

### Audio Not Playing
Verify `staticwebapp.config.json` is in `dist` folder

### 404 on Refresh
Ensure `navigationFallback` is configured in `staticwebapp.config.json`

## 📚 Full Documentation

See [AZURE_DEPLOYMENT_GUIDE.md](AZURE_DEPLOYMENT_GUIDE.md) for complete instructions.

## 💰 Cost

**Free tier**: $0/month (sufficient for this app)

If you exceed limits: Standard tier is $9/month

## 🔄 Updates

Push to GitHub → Automatic deployment in 2-5 minutes

## 🌐 Custom Domain (Optional)

1. Azure Portal → Your app → Custom domains → Add
2. Add DNS records at your domain registrar
3. SSL certificate automatically provisioned

## 📞 Need Help?

- [Azure Static Web Apps Docs](https://docs.microsoft.com/azure/static-web-apps/)
- [Full Deployment Guide](AZURE_DEPLOYMENT_GUIDE.md)
- [GitHub Issues](https://github.com/<username>/<repo>/issues)
