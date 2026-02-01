# Azure Deployment Fix Applied

## Issue Encountered

When deploying to Azure Static Web Apps, you encountered this error:

```
---End of Oryx build logs---
Oryx was unable to determine the build steps. Continuing assuming the assets in this folder are already built. If this is an unexpected behavior please contact support.
Finished building app with Oryx
Failed to find a default file in the app artifacts folder (/). Valid default files: index.html,Index.html.
If your application contains purely static content, please verify that the variable 'app_location' in your workflow file points to the root of your application.
If your application requires build steps, please validate that a default file exists in the build output directory.
```

## Root Cause

The GitHub Actions workflow was configured incorrectly:
- `app_location` was set to `src/web/client/dist` (the build output)
- `skip_app_build` was set to `true`
- Manual build steps were included before the Azure deploy action

This caused Azure to look for pre-built files in the dist folder, but since the dist folder is generated during build, it wasn't in the repository.

## Solution Applied

Updated `.github/workflows/azure-static-web-apps.yml` with the correct configuration:

### Before (Incorrect)
```yaml
- name: Setup Node.js
  uses: actions/setup-node@v3
  with:
    node-version: '18'
    cache: 'npm'
    cache-dependency-path: src/web/client/package-lock.json

- name: Install dependencies
  run: |
    cd src/web/client
    npm ci

- name: Build application
  run: |
    cd src/web/client
    npm run build

- name: Build And Deploy
  uses: Azure/static-web-apps-deploy@v1
  with:
    azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
    repo_token: ${{ secrets.GITHUB_TOKEN }}
    action: "upload"
    app_location: "src/web/client/dist"  # ❌ Wrong - points to build output
    skip_app_build: true                  # ❌ Wrong - skips Azure's build
    output_location: ""
```

### After (Correct)
```yaml
- name: Build And Deploy
  uses: Azure/static-web-apps-deploy@v1
  with:
    azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
    repo_token: ${{ secrets.GITHUB_TOKEN }}
    action: "upload"
    app_location: "src/web/client"       # ✅ Correct - points to source
    api_location: ""                      # ✅ No API
    output_location: "dist"               # ✅ Correct - build output folder
```

## Key Changes

1. **Removed manual build steps** - Azure Static Web Apps handles the build automatically using Oryx
2. **Changed `app_location`** from `src/web/client/dist` to `src/web/client`
3. **Removed `skip_app_build: true`** - Let Azure build the app
4. **Added `output_location: "dist"`** - Tells Azure where to find the built files
5. **Removed Node.js setup** - Azure provides the build environment

## How Azure Static Web Apps Works

Azure Static Web Apps uses **Oryx** to automatically detect and build your application:

1. **Detection**: Oryx looks at `app_location` (src/web/client)
2. **Identifies**: Finds `package.json` and detects it's a Node.js app
3. **Installs**: Runs `npm install` automatically
4. **Builds**: Runs `npm run build` automatically
5. **Deploys**: Takes files from `output_location` (dist) and deploys them

## What to Do Now

1. **Commit the fix**:
   ```bash
   cd C:\sai\dev\gita
   git add .github/workflows/azure-static-web-apps.yml
   git commit -m "Fix Azure deployment workflow configuration"
   git push origin main
   ```

2. **Monitor the deployment**:
   - Go to GitHub → Actions tab
   - Watch the new workflow run
   - It should now build and deploy successfully

3. **Verify deployment**:
   - Once complete, visit your Azure URL
   - Test the application

## Expected Build Output

You should now see in GitHub Actions:

```
Detecting platforms...
Detected following platforms:
  nodejs: 18.x

Building nodejs app...
Running 'npm install'...
Running 'npm run build'...

Build successful!
Deploying to Azure Static Web Apps...
Deployment complete!
```

## Configuration Summary

### Correct Workflow Configuration
```yaml
app_location: "src/web/client"    # Where package.json is
api_location: ""                   # No API
output_location: "dist"            # Where build outputs to
```

### Azure Portal Configuration
When you created the Static Web App in Azure Portal, you should have:
- **App location**: `src/web/client` (not `src/web/client/dist`)
- **Output location**: `dist`
- **API location**: (empty)

If you set it differently in Azure Portal, the workflow file now overrides those settings correctly.

## Verification Checklist

After pushing the fix:

- [ ] GitHub Actions workflow runs without errors
- [ ] Build step completes successfully
- [ ] Deployment step completes successfully
- [ ] Application is accessible at Azure URL
- [ ] All 19 chapters load correctly
- [ ] Audio files play correctly
- [ ] No console errors in browser

## Common Mistakes to Avoid

### ❌ Don't Do This
```yaml
app_location: "src/web/client/dist"  # Points to build output (doesn't exist in repo)
skip_app_build: true                  # Skips Azure's automatic build
```

### ✅ Do This
```yaml
app_location: "src/web/client"       # Points to source code
output_location: "dist"               # Where build outputs
# Let Azure build automatically
```

## Additional Notes

### Why Not Build Manually?

While you *can* build manually in GitHub Actions and then deploy, it's better to let Azure handle it because:

1. **Simpler workflow** - Less code to maintain
2. **Azure optimizations** - Oryx applies Azure-specific optimizations
3. **Consistent environment** - Same build environment as Azure uses
4. **Better caching** - Azure caches dependencies between builds
5. **Easier debugging** - Azure provides detailed build logs

### When to Build Manually

You might want to build manually if:
- You need custom build steps not supported by Oryx
- You need to run tests before deployment
- You need to use specific Node.js versions not available in Azure
- You need to modify files after build

For this application, Azure's automatic build is perfect.

## Troubleshooting

### If Build Still Fails

1. **Check package.json location**:
   ```bash
   # Should be at: src/web/client/package.json
   ```

2. **Verify build script exists**:
   ```json
   {
     "scripts": {
       "build": "vite build"  // Must exist
     }
   }
   ```

3. **Check Azure logs**:
   - Go to Azure Portal → Your Static Web App
   - Navigate to "Deployment history"
   - Click on the failed deployment
   - Review detailed logs

4. **Verify secret is set**:
   - GitHub → Settings → Secrets → Actions
   - `AZURE_STATIC_WEB_APPS_API_TOKEN` should exist

### If Deployment Succeeds But App Doesn't Work

1. **Check staticwebapp.config.json**:
   - Should be in `src/web/client/public/`
   - Will be copied to dist during build

2. **Verify data files**:
   - Should be in `src/web/client/public/data/`
   - Will be copied to dist during build

3. **Check browser console**:
   - Open DevTools → Console
   - Look for errors

## Success!

Once you push the fixed workflow, your deployment should succeed. The application will be built automatically by Azure and deployed to your Static Web App URL.

---

**Fix Applied**: February 1, 2026  
**Issue**: Incorrect app_location configuration  
**Solution**: Point to source directory, let Azure build automatically  
**Status**: ✅ Ready to deploy
