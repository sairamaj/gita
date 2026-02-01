# Azure Static Web Apps - Deployment Checklist

Use this checklist to ensure successful deployment to Azure.

## Pre-Deployment

### Local Preparation
- [x] Application builds successfully (`npm run build`)
- [x] Dist folder size verified (~159 MB, under 250 MB limit)
- [x] `staticwebapp.config.json` created and configured
- [x] GitHub Actions workflow created (`.github/workflows/azure-static-web-apps.yml`)
- [ ] Code committed to Git
- [ ] Code pushed to GitHub repository

### Azure Account
- [ ] Azure account created and active
- [ ] Subscription selected
- [ ] Resource group created or identified

### GitHub Account
- [ ] GitHub account created
- [ ] Repository created
- [ ] Code pushed to repository

## Azure Portal Setup

### Create Static Web App Resource
- [ ] Signed in to Azure Portal (https://portal.azure.com)
- [ ] Clicked "Create a resource"
- [ ] Searched for "Static Web Apps"
- [ ] Clicked "Create"

### Basic Configuration
- [ ] Subscription selected
- [ ] Resource Group: Created or selected (e.g., `gita-practice-rg`)
- [ ] Name: Entered unique name (e.g., `gita-practice-app`)
- [ ] Plan type: Selected "Free"
- [ ] Region: Selected (closest to users)

### Deployment Configuration
- [ ] Source: Selected "GitHub"
- [ ] Authorized GitHub access
- [ ] Organization: Selected
- [ ] Repository: Selected
- [ ] Branch: Selected (e.g., `main` or `feature/spa`)

### Build Configuration
- [ ] Build Presets: Selected "Custom"
- [ ] App location: Set to `src/web/client/dist`
- [ ] Api location: Left empty
- [ ] Output location: Left empty

### Finalize
- [ ] Clicked "Review + create"
- [ ] Verified all settings
- [ ] Clicked "Create"
- [ ] Waited for deployment to complete (2-3 minutes)

### Post-Creation
- [ ] Noted the app URL (e.g., `https://gita-practice-app.azurestaticapps.net`)
- [ ] Navigated to "Configuration" → "Deployment token"
- [ ] Copied deployment token

## GitHub Configuration

### Add Deployment Secret
- [ ] Went to GitHub repository
- [ ] Clicked "Settings" tab
- [ ] Navigated to "Secrets and variables" → "Actions"
- [ ] Clicked "New repository secret"
- [ ] Name: `AZURE_STATIC_WEB_APPS_API_TOKEN`
- [ ] Value: Pasted deployment token from Azure
- [ ] Clicked "Add secret"

### Verify Workflow File
- [ ] Confirmed `.github/workflows/azure-static-web-apps.yml` exists
- [ ] Verified app_location is `src/web/client/dist`
- [ ] Verified output_location is empty
- [ ] Verified skip_app_build is true

## Deployment

### Initial Deployment
- [ ] Committed all changes:
  ```bash
  git add .
  git commit -m "Add Azure deployment configuration"
  ```
- [ ] Pushed to GitHub:
  ```bash
  git push origin main
  ```
- [ ] Navigated to GitHub Actions tab
- [ ] Watched "Azure Static Web Apps CI/CD" workflow
- [ ] Verified workflow completed successfully
- [ ] Noted deployment time

### Verify Build
- [ ] Build step completed without errors
- [ ] Deploy step completed without errors
- [ ] No warnings in logs (or acceptable warnings)

## Post-Deployment Verification

### Access Application
- [ ] Opened Azure app URL in browser
- [ ] Application loaded without errors
- [ ] No console errors in browser DevTools

### Functionality Testing
- [ ] Chapter selector displays all 19 chapters
- [ ] Selected different chapters successfully
- [ ] Chapter metadata loads correctly
- [ ] Audio files load and play
- [ ] Audio playback controls work (play, pause, speed)

### Individual Practice Mode
- [ ] Switched to Individual Practice tab
- [ ] Selected a chapter
- [ ] Clicked "Play" button
- [ ] Device plays shloka correctly
- [ ] "Your turn" indicator appears
- [ ] Wait mode works (keyboard/duration)
- [ ] "Finish Your turn" button works (keyboard mode)
- [ ] "Repeat your shloka" option works
- [ ] Playback speed adjustment works
- [ ] "Stop" button works

### Group Practice Mode
- [ ] Switched to Group Practice tab
- [ ] Selected a chapter
- [ ] Configured participants (e.g., 4)
- [ ] Set "Your turn" position (e.g., 2)
- [ ] Clicked "Play" button
- [ ] Participants take turns correctly
- [ ] "Your turn" highlights correctly
- [ ] Wait mode works
- [ ] All controls function properly
- [ ] "Stop" button works

### Help Section
- [ ] Switched to Help tab
- [ ] Individual practice help displays
- [ ] Group practice help displays
- [ ] Markdown formatting correct

### Cross-Browser Testing
- [ ] Tested on Chrome
- [ ] Tested on Firefox
- [ ] Tested on Safari (if available)
- [ ] Tested on Edge

### Mobile Testing (if applicable)
- [ ] Opened on mobile device
- [ ] Responsive design works
- [ ] Touch controls work
- [ ] Audio plays on mobile
- [ ] All features accessible

### Performance
- [ ] Initial load time acceptable
- [ ] Audio loads without long delays
- [ ] No lag in UI interactions
- [ ] Network tab shows no failed requests

## Configuration (Optional)

### Custom Domain
- [ ] Decided on custom domain
- [ ] Added custom domain in Azure Portal
- [ ] Configured DNS records at domain registrar
- [ ] Waited for DNS propagation
- [ ] Validated domain in Azure
- [ ] SSL certificate automatically provisioned
- [ ] Tested custom domain URL

### Monitoring
- [ ] Enabled Application Insights (if desired)
- [ ] Set up bandwidth monitoring
- [ ] Configured cost alerts
- [ ] Set up uptime monitoring

## Documentation

### Update Documentation
- [ ] Updated README with deployment URL
- [ ] Documented any custom configuration
- [ ] Noted deployment date and details
- [ ] Shared URL with team/users

### Record Information
- **Deployment Date**: _______________
- **Azure URL**: _______________
- **Custom Domain**: _______________
- **Resource Group**: _______________
- **Region**: _______________
- **Plan Type**: Free / Standard

## Troubleshooting (if needed)

### If Build Fails
- [ ] Checked GitHub Actions logs
- [ ] Verified Node.js version (18+)
- [ ] Confirmed package-lock.json is committed
- [ ] Checked for build errors locally
- [ ] Re-ran build locally to reproduce

### If Audio Doesn't Play
- [ ] Verified `staticwebapp.config.json` in dist folder
- [ ] Checked MIME type configuration
- [ ] Confirmed audio files in `dist/data/` folder
- [ ] Tested in different browser
- [ ] Checked browser console for errors

### If 404 on Refresh
- [ ] Verified `navigationFallback` in config
- [ ] Confirmed config file deployed to Azure
- [ ] Rebuilt and redeployed

### If Deployment Token Invalid
- [ ] Regenerated token in Azure Portal
- [ ] Updated GitHub secret
- [ ] Re-ran workflow

## Maintenance

### Regular Tasks
- [ ] Monitor bandwidth usage monthly
- [ ] Review cost reports
- [ ] Check for security updates
- [ ] Update dependencies periodically
- [ ] Test application after updates

### Update Process
- [ ] Make code changes
- [ ] Test locally
- [ ] Commit and push to GitHub
- [ ] Monitor automatic deployment
- [ ] Verify changes in production

## Success Criteria

Deployment is successful when:
- ✅ Application loads at Azure URL
- ✅ All 19 chapters accessible
- ✅ Audio playback works
- ✅ Both practice modes function
- ✅ No console errors
- ✅ Works on multiple browsers
- ✅ Performance is acceptable
- ✅ Automatic deployments work

## Next Steps

- [ ] Share deployment URL with users
- [ ] Set up monitoring and alerts
- [ ] Plan for custom domain (if needed)
- [ ] Document any issues encountered
- [ ] Create user documentation
- [ ] Gather user feedback

---

**Completed By**: _______________  
**Date**: _______________  
**Notes**: _______________
