# Deployment Guide for Gita Practice Web App

This guide provides step-by-step instructions for deploying the Gita Practice web application as a static website to various hosting platforms.

## Prerequisites

1. Node.js and npm installed
2. The application built using `npm run build` in `src/web/client`
3. All files in the `dist` folder ready for deployment

## Build Instructions

Before deploying, build the application:

```bash
cd src/web/client
npm install
npm run build
```

This creates a `dist` folder containing:
- `index.html` - Main HTML file
- `assets/` - JavaScript and CSS bundles
- `data/` - All chapter metadata and audio files (19 chapters, ~170 MB)
- `chapters.json` - Chapter list configuration
- `config.json` - Application settings

## Deployment Options

### 1. Azure Static Web Apps

**Via Azure Portal:**

1. Sign in to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → "Static Web App"
3. Fill in the details:
   - Subscription: Select your subscription
   - Resource Group: Create new or select existing
   - Name: `gita-practice-app` (or your preferred name)
   - Region: Choose closest to your users
   - Deployment source: Choose "Other" for manual deployment
4. Click "Review + create" → "Create"
5. Once created, go to the resource
6. Under "Settings" → "Configuration", note the deployment token
7. Upload the `dist` folder contents using Azure CLI or GitHub Actions

**Via Azure CLI:**

```bash
# Install Azure Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Deploy
cd src/web/client
swa deploy ./dist --deployment-token <YOUR_DEPLOYMENT_TOKEN>
```

**Via GitHub Actions:**

Create `.github/workflows/azure-static-web-apps.yml`:

```yaml
name: Azure Static Web Apps CI/CD

on:
  push:
    branches:
      - main
  pull_request:
    types: [opened, synchronize, reopened, closed]
    branches:
      - main

jobs:
  build_and_deploy_job:
    runs-on: ubuntu-latest
    name: Build and Deploy Job
    steps:
      - uses: actions/checkout@v3
        with:
          submodules: true
      - name: Build And Deploy
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "src/web/client"
          output_location: "dist"
```

### 2. AWS S3 + CloudFront

**Step 1: Create S3 Bucket**

```bash
# Create bucket
aws s3 mb s3://gita-practice-app

# Enable static website hosting
aws s3 website s3://gita-practice-app --index-document index.html --error-document index.html

# Upload files
cd src/web/client/dist
aws s3 sync . s3://gita-practice-app --acl public-read
```

**Step 2: Configure Bucket Policy**

Create a bucket policy for public read access:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::gita-practice-app/*"
    }
  ]
}
```

**Step 3: (Optional) Set up CloudFront**

1. Go to CloudFront in AWS Console
2. Create a new distribution
3. Set origin to your S3 bucket
4. Configure default root object as `index.html`
5. Set error pages to redirect to `index.html` (for SPA routing)
6. Deploy and note the CloudFront URL

### 3. Netlify

**Via Netlify CLI:**

```bash
# Install Netlify CLI
npm install -g netlify-cli

# Deploy
cd src/web/client
netlify deploy --prod --dir=dist
```

**Via Netlify UI:**

1. Sign in to [Netlify](https://app.netlify.com)
2. Click "Add new site" → "Deploy manually"
3. Drag and drop the `dist` folder
4. Your site will be deployed instantly

**Via Git Integration:**

1. Push your code to GitHub/GitLab/Bitbucket
2. In Netlify, click "Add new site" → "Import an existing project"
3. Connect your repository
4. Configure build settings:
   - Base directory: `src/web/client`
   - Build command: `npm run build`
   - Publish directory: `src/web/client/dist`
5. Click "Deploy site"

### 4. Vercel

**Via Vercel CLI:**

```bash
# Install Vercel CLI
npm install -g vercel

# Deploy
cd src/web/client
vercel --prod
```

**Via Vercel UI:**

1. Sign in to [Vercel](https://vercel.com)
2. Click "Add New" → "Project"
3. Import your Git repository
4. Configure:
   - Framework Preset: Vite
   - Root Directory: `src/web/client`
   - Build Command: `npm run build`
   - Output Directory: `dist`
5. Click "Deploy"

### 5. GitHub Pages

**Option A: Using gh-pages package**

```bash
# Install gh-pages
cd src/web/client
npm install --save-dev gh-pages

# Add deploy script to package.json
# "scripts": {
#   "deploy": "npm run build && gh-pages -d dist"
# }

# Deploy
npm run deploy
```

**Option B: Manual deployment**

```bash
# Build the app
cd src/web/client
npm run build

# Create gh-pages branch
git checkout --orphan gh-pages
git rm -rf .
cp -r dist/* .
git add .
git commit -m "Deploy to GitHub Pages"
git push origin gh-pages --force

# Switch back to main branch
git checkout main
```

Then enable GitHub Pages in repository settings:
1. Go to Settings → Pages
2. Source: Deploy from a branch
3. Branch: `gh-pages` / `root`
4. Save

### 6. Firebase Hosting

```bash
# Install Firebase CLI
npm install -g firebase-tools

# Login to Firebase
firebase login

# Initialize Firebase in your project
cd src/web/client
firebase init hosting

# Configure:
# - Public directory: dist
# - Single-page app: Yes
# - Overwrite index.html: No

# Deploy
npm run build
firebase deploy --only hosting
```

## Post-Deployment Checklist

After deploying, verify the following:

- [ ] Application loads correctly
- [ ] All chapters are listed in the dropdown
- [ ] Chapter metadata loads when selected
- [ ] Audio files play correctly
- [ ] Individual practice mode works
- [ ] Group practice mode works
- [ ] Help section displays properly
- [ ] All controls (playback speed, wait mode, etc.) function correctly

## Troubleshooting

### Audio files not loading

- Ensure MIME types are configured correctly on your hosting platform
- M4A files should be served with `audio/mp4` or `audio/x-m4a` MIME type

### 404 errors for routes

- Configure your hosting platform to redirect all routes to `index.html`
- This is necessary for single-page applications

### Large initial load time

- Consider enabling compression (gzip/brotli) on your hosting platform
- Use a CDN for faster content delivery
- The initial load includes all audio files (~170 MB), which is expected

### CORS errors

- Ensure your hosting platform allows cross-origin requests
- This shouldn't be an issue for static files served from the same domain

## Custom Domain

Most hosting platforms support custom domains:

1. **Azure Static Web Apps**: Settings → Custom domains → Add
2. **AWS S3 + CloudFront**: Use Route 53 or your DNS provider
3. **Netlify**: Site settings → Domain management → Add custom domain
4. **Vercel**: Project settings → Domains → Add
5. **GitHub Pages**: Settings → Pages → Custom domain

## Performance Optimization

For better performance:

1. **Enable compression**: Most platforms enable gzip/brotli by default
2. **Set cache headers**: Configure long cache times for static assets
3. **Use CDN**: Distribute content globally for faster access
4. **Lazy load audio**: Consider implementing lazy loading for audio files (future enhancement)

## Cost Considerations

- **Azure Static Web Apps**: Free tier available (100 GB bandwidth/month)
- **AWS S3 + CloudFront**: Pay per use (~$1-5/month for moderate traffic)
- **Netlify**: Free tier available (100 GB bandwidth/month)
- **Vercel**: Free tier available (100 GB bandwidth/month)
- **GitHub Pages**: Free for public repositories
- **Firebase Hosting**: Free tier available (10 GB storage, 360 MB/day transfer)

Note: The application includes ~170 MB of audio files, so bandwidth usage may be significant with many users.

## Security

Since this is a static site with no backend:

- No server-side vulnerabilities
- No database to secure
- No API keys to protect
- HTTPS is automatically provided by most hosting platforms

## Maintenance

The application requires no server maintenance. To update:

1. Make changes to the source code
2. Rebuild: `npm run build`
3. Redeploy the `dist` folder

For automated deployments, use CI/CD pipelines (GitHub Actions, Azure Pipelines, etc.).
