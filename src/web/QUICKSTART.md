# Quick Start Guide - Gita Practice Static Web App

## For Developers

### Local Development

```bash
# Navigate to the client directory
cd src/web/client

# Install dependencies (first time only)
npm install

# Start development server
npm run dev

# Open browser to http://localhost:5173
```

### Build for Production

```bash
# Build the application
npm run build

# Preview the production build locally
npm run preview
```

The production files will be in the `dist` folder.

## For Deployment

### Quick Deploy to Netlify (Easiest)

```bash
# Install Netlify CLI
npm install -g netlify-cli

# Build and deploy
cd src/web/client
npm run build
netlify deploy --prod --dir=dist
```

### Quick Deploy to Vercel

```bash
# Install Vercel CLI
npm install -g vercel

# Deploy
cd src/web/client
vercel --prod
```

### Manual Deployment

1. Build the application:
   ```bash
   cd src/web/client
   npm run build
   ```

2. Upload the entire `dist` folder to your hosting service:
   - Azure Static Web Apps
   - AWS S3
   - GitHub Pages
   - Firebase Hosting
   - Any static hosting service

## What's Included

The application is completely self-contained:

- ✅ All 19 chapters of Bhagavad Gita
- ✅ Audio files for each chapter (~170 MB total)
- ✅ Metadata with shloka timings
- ✅ Individual practice mode
- ✅ Group practice mode
- ✅ No backend server required
- ✅ No external API dependencies

## Folder Structure

```
src/web/client/
├── public/              # Static assets (bundled in build)
│   ├── data/           # All chapter data
│   ├── chapters.json   # Chapter list
│   └── config.json     # App settings
├── src/                # React source code
├── dist/               # Production build (after npm run build)
└── package.json
```

## Troubleshooting

### Build fails
- Make sure you're in `src/web/client` directory
- Run `npm install` first
- Check Node.js version (requires Node 14+)

### Audio not playing
- Check browser console for errors
- Ensure audio files are in `public/data/` directory
- Verify MIME types are correct on hosting platform

### Files not found (404)
- Make sure you deployed the entire `dist` folder
- Check that `data` directory is included
- Verify paths are relative (starting with `/`)

## Next Steps

- See [README.md](README.md) for detailed documentation
- See [DEPLOYMENT.md](DEPLOYMENT.md) for platform-specific deployment guides
- See [MIGRATION_NOTES.md](MIGRATION_NOTES.md) for technical details about the static architecture

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review the detailed documentation files
3. Check browser console for error messages
4. Verify all files are deployed correctly

## Performance Tips

- Enable gzip/brotli compression on your hosting platform
- Use a CDN for faster global access
- Set long cache headers for static assets
- Consider implementing lazy loading for audio files (future enhancement)

## Browser Requirements

- Modern browsers with ES6+ support
- Audio playback support for M4A format
- Tested on: Chrome, Firefox, Safari, Edge
