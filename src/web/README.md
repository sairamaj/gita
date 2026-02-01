# Gita Practice Web

A pure static web application for Bhagavad Gita recitation practice with individual and group modes.

## Features

- **Pure Static Site**: No backend server required - all data is bundled with the application
- **Individual Practice Mode**: Practice with the device playing shlokas
- **Group Practice Mode**: Simulate group recitation with multiple participants
- **Offline Capable**: All audio and metadata files are included
- **Easy Deployment**: Deploy to any static hosting service (Azure Static Web Apps, AWS S3, Netlify, Vercel, GitHub Pages, etc.)

## Quick Start

### Development

1. Install dependencies:

```bash
cd src/web/client
npm install
```

2. Start the development server:

```bash
npm run dev
```

3. Open your browser to `http://localhost:5173`

### Production Build

Build the static site:

```bash
cd src/web/client
npm run build
```

The production-ready files will be in the `dist` folder, including:
- All HTML, CSS, and JavaScript files
- Configuration files (`config.json`, `chapters.json`)
- All chapter metadata and audio files in the `data` directory

### Preview Production Build

Test the production build locally:

```bash
npm run preview
```

## Deployment

The application can be deployed to any static hosting service. Simply upload the contents of the `dist` folder.

### Azure Static Web Apps

1. Create a new Static Web App in Azure Portal
2. Connect to your repository or upload the `dist` folder directly
3. Set build configuration:
   - App location: `src/web/client`
   - Output location: `dist`

### AWS S3 + CloudFront

1. Create an S3 bucket and enable static website hosting
2. Upload the contents of the `dist` folder
3. (Optional) Set up CloudFront for CDN distribution
4. Configure bucket policy for public read access

### Netlify / Vercel

1. Connect your repository
2. Set build command: `npm run build`
3. Set publish directory: `src/web/client/dist`

### GitHub Pages

1. Build the application: `npm run build`
2. Push the `dist` folder contents to the `gh-pages` branch
3. Enable GitHub Pages in repository settings

## Project Structure

```
src/web/client/
├── public/              # Static assets (copied to dist during build)
│   ├── data/           # Chapter metadata and audio files
│   │   ├── 00/         # Chapter 0 (Gita Dhayna Slokas)
│   │   ├── 01/         # Chapter 1
│   │   └── ...         # Chapters 2-18
│   ├── chapters.json   # Chapter list
│   └── config.json     # Application configuration
├── src/
│   ├── components/     # React components
│   ├── api.js         # Data loading functions
│   ├── App.jsx        # Main application component
│   └── main.jsx       # Application entry point
├── dist/              # Production build output (generated)
└── package.json
```

## Data Files

All chapter data is included in the application:
- **Metadata**: JSON files with shloka timings and text (`plain_chapter.json`)
- **Audio**: M4A audio files for each chapter (`plain_chapter.m4a`)
- **Total Size**: ~170 MB (all 19 chapters)

## Configuration

Edit `public/config.json` to customize default settings:

```json
{
  "defaults": {
    "waitMode": "keyboard",
    "duration": 20,
    "playbackSpeed": 1.5,
    "repeatYourShloka": true,
    "participants": 4,
    "yourTurn": 2
  }
}
```

## Browser Compatibility

- Modern browsers with ES6+ support
- Audio playback support for M4A format
- Tested on Chrome, Firefox, Safari, and Edge
