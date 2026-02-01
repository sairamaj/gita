# Deployment Checklist

Use this checklist to ensure a successful deployment of the Gita Practice static web application.

## Pre-Deployment

### 1. Build Preparation
- [ ] Navigate to `src/web/client` directory
- [ ] Run `npm install` to ensure all dependencies are installed
- [ ] Run `npm run build` to create production build
- [ ] Verify build completed without errors
- [ ] Check that `dist/` folder was created

### 2. Build Verification
- [ ] Verify `dist/index.html` exists
- [ ] Verify `dist/assets/` folder contains JS and CSS files
- [ ] Verify `dist/data/` folder contains all 19 chapters (00-18)
- [ ] Verify `dist/chapters.json` exists
- [ ] Verify `dist/config.json` exists
- [ ] Check total size is approximately 159-170 MB

### 3. Local Testing
- [ ] Run `npm run preview` to test production build locally
- [ ] Open browser to the preview URL (usually http://localhost:4173)
- [ ] Verify application loads without errors
- [ ] Check browser console for any errors
- [ ] Test chapter selection (select at least 2-3 different chapters)
- [ ] Test audio playback for at least one chapter
- [ ] Test Individual Practice mode
- [ ] Test Group Practice mode
- [ ] Verify all controls work (play, stop, speed, wait mode, etc.)
- [ ] Check Help section displays correctly

## Deployment

### 4. Choose Hosting Platform
Select one:
- [ ] Azure Static Web Apps
- [ ] AWS S3 + CloudFront
- [ ] Netlify
- [ ] Vercel
- [ ] GitHub Pages
- [ ] Firebase Hosting
- [ ] Other: _______________

### 5. Platform-Specific Setup
Refer to `DEPLOYMENT.md` for detailed instructions for your chosen platform.

#### For Azure Static Web Apps:
- [ ] Create Static Web App resource in Azure Portal
- [ ] Configure deployment source
- [ ] Set build configuration (app location: `src/web/client`, output: `dist`)
- [ ] Deploy using CLI or GitHub Actions

#### For AWS S3:
- [ ] Create S3 bucket
- [ ] Enable static website hosting
- [ ] Configure bucket policy for public read
- [ ] Upload `dist/` contents
- [ ] (Optional) Set up CloudFront distribution

#### For Netlify:
- [ ] Install Netlify CLI: `npm install -g netlify-cli`
- [ ] Run: `netlify deploy --prod --dir=dist`
- [ ] Or connect Git repository for automatic deployments

#### For Vercel:
- [ ] Install Vercel CLI: `npm install -g vercel`
- [ ] Run: `vercel --prod`
- [ ] Or connect Git repository for automatic deployments

#### For GitHub Pages:
- [ ] Build the application
- [ ] Create `gh-pages` branch or use existing
- [ ] Push `dist/` contents to `gh-pages` branch
- [ ] Enable GitHub Pages in repository settings

#### For Firebase:
- [ ] Install Firebase CLI: `npm install -g firebase-tools`
- [ ] Run: `firebase login`
- [ ] Run: `firebase init hosting`
- [ ] Configure public directory as `dist`
- [ ] Run: `firebase deploy --only hosting`

### 6. Upload/Deploy
- [ ] Upload entire `dist/` folder contents to hosting platform
- [ ] Wait for deployment to complete
- [ ] Note the deployment URL

## Post-Deployment

### 7. Verification
- [ ] Open the deployed URL in browser
- [ ] Clear browser cache (Ctrl+Shift+R or Cmd+Shift+R)
- [ ] Verify application loads correctly
- [ ] Check browser console for errors
- [ ] Test chapter selection
- [ ] Test audio playback (try multiple chapters)
- [ ] Test Individual Practice mode:
  - [ ] Play button works
  - [ ] Audio plays correctly
  - [ ] Stop button works
  - [ ] Wait mode (keyboard/duration) works
  - [ ] Playback speed adjustment works
  - [ ] Repeat your shloka option works
- [ ] Test Group Practice mode:
  - [ ] Play button works
  - [ ] Participant turns work correctly
  - [ ] "Your turn" indicator works
  - [ ] Stop button works
  - [ ] All controls work
- [ ] Test Help section
- [ ] Test on mobile device (if applicable)
- [ ] Test on different browsers (Chrome, Firefox, Safari, Edge)

### 8. Performance Check
- [ ] Check page load time (should be reasonable after first load)
- [ ] Verify audio files load and play smoothly
- [ ] Check network tab for any failed requests
- [ ] Verify all resources are served with correct MIME types
- [ ] Check if compression (gzip/brotli) is enabled

### 9. Configuration
- [ ] Verify default settings are appropriate (check `config.json`)
- [ ] Configure custom domain (if needed)
- [ ] Set up SSL/HTTPS (usually automatic on most platforms)
- [ ] Configure cache headers (if platform allows)
- [ ] Set up CDN (if not already included)

### 10. Monitoring & Analytics (Optional)
- [ ] Set up analytics (Google Analytics, Plausible, etc.)
- [ ] Configure error tracking (Sentry, etc.)
- [ ] Set up uptime monitoring
- [ ] Configure alerts for downtime

### 11. Documentation
- [ ] Update deployment URL in documentation
- [ ] Share deployment URL with team/users
- [ ] Document any custom configuration
- [ ] Note any platform-specific settings

## Troubleshooting

If you encounter issues, check:

### Audio Not Playing
- [ ] Verify audio files are in `dist/data/` directory
- [ ] Check browser console for 404 errors
- [ ] Verify MIME type for M4A files is correct (`audio/mp4` or `audio/x-m4a`)
- [ ] Try different browser
- [ ] Check if browser supports M4A format

### 404 Errors
- [ ] Verify all files were uploaded
- [ ] Check file paths are correct (relative paths starting with `/`)
- [ ] Verify `data/` directory structure is intact
- [ ] Check hosting platform configuration

### Slow Loading
- [ ] Enable compression on hosting platform
- [ ] Use CDN for faster delivery
- [ ] Check network speed
- [ ] Verify files are being cached properly

### Configuration Not Loading
- [ ] Verify `chapters.json` and `config.json` are in root of `dist/`
- [ ] Check browser console for JSON parsing errors
- [ ] Verify JSON files are valid (use JSON validator)

### Build Errors
- [ ] Run `npm install` again
- [ ] Clear `node_modules/` and reinstall: `rm -rf node_modules && npm install`
- [ ] Check Node.js version (requires 14+)
- [ ] Verify all source files are present

## Rollback Plan

If deployment fails or has critical issues:

- [ ] Keep previous deployment available
- [ ] Have backup of working `dist/` folder
- [ ] Know how to revert to previous version on your platform
- [ ] Document rollback procedure for your platform

## Success Criteria

Deployment is successful when:

- ✅ Application loads without errors
- ✅ All chapters are accessible
- ✅ Audio playback works correctly
- ✅ Both practice modes function properly
- ✅ All controls work as expected
- ✅ No console errors
- ✅ Performance is acceptable
- ✅ Works on multiple browsers
- ✅ Mobile-friendly (if applicable)

## Next Steps After Deployment

- [ ] Share deployment URL with users
- [ ] Set up CI/CD for automatic deployments (optional)
- [ ] Monitor usage and performance
- [ ] Gather user feedback
- [ ] Plan future enhancements

## Notes

- Total deployment size: ~159-170 MB (mostly audio files)
- First load may be slow due to audio file downloads
- Subsequent loads should be faster due to browser caching
- Consider implementing PWA features for offline support (future enhancement)

## Resources

- See `DEPLOYMENT.md` for detailed platform-specific guides
- See `QUICKSTART.md` for quick commands
- See `README.md` for application overview
- See `MIGRATION_NOTES.md` for technical details

---

**Date Deployed:** _______________  
**Platform:** _______________  
**Deployment URL:** _______________  
**Deployed By:** _______________  
