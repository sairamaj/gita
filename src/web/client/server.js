import express from 'express';
import fs from 'fs/promises';
import path from 'path';
import { randomUUID } from 'crypto';
import { fileURLToPath } from 'url';
import { dirname } from 'path';
import {
  QUICK_PRACTICE_API_BASE,
  validateQuickPracticeCreatePayload,
} from './src/contracts/quickPracticeContracts.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const QUICK_PRACTICE_DATA_DIR = path.join(__dirname, 'data');
const QUICK_PRACTICE_DATA_FILE = path.join(QUICK_PRACTICE_DATA_DIR, 'quick-practice.json');

const app = express();
const PORT = process.env.PORT || 8080;
let quickPracticeItems = [];
let writeQueue = Promise.resolve();

app.use(express.json());

async function ensureQuickPracticeDataDir() {
  await fs.mkdir(QUICK_PRACTICE_DATA_DIR, { recursive: true });
}

async function loadQuickPracticeStore() {
  await ensureQuickPracticeDataDir();

  try {
    const raw = await fs.readFile(QUICK_PRACTICE_DATA_FILE, 'utf8');
    const parsed = JSON.parse(raw);
    if (!parsed || !Array.isArray(parsed.quickPracticeItems)) {
      throw new Error('Storage file has invalid shape.');
    }
    quickPracticeItems = parsed.quickPracticeItems;
  } catch (error) {
    if (error.code === 'ENOENT') {
      quickPracticeItems = [];
      return;
    }

    quickPracticeItems = [];
    console.warn(
      `Quick Practice storage load failed (${error.message}). Continuing with empty list.`
    );
  }
}

function queueQuickPracticeWrite() {
  writeQueue = writeQueue
    .then(async () => {
      await ensureQuickPracticeDataDir();
      const payload = JSON.stringify({ quickPracticeItems }, null, 2);
      await fs.writeFile(QUICK_PRACTICE_DATA_FILE, payload, 'utf8');
    })
    .catch((error) => {
      console.error(`Quick Practice write failed: ${error.message}`);
      throw error;
    });

  return writeQueue;
}

app.get(QUICK_PRACTICE_API_BASE, (_req, res) => {
  res.json({ quickPracticeItems });
});

app.post(QUICK_PRACTICE_API_BASE, async (req, res) => {
  const validation = validateQuickPracticeCreatePayload(req.body);
  if (!validation.valid) {
    res.status(400).json({ message: validation.message });
    return;
  }

  const item = {
    id: randomUUID(),
    chapterNumber: req.body.chapterNumber,
    slokaNumber: req.body.slokaNumber,
    createdAt: new Date().toISOString(),
  };

  quickPracticeItems.push(item);

  try {
    await queueQuickPracticeWrite();
    res.status(201).json({ item });
  } catch (_error) {
    quickPracticeItems.pop();
    res.status(500).json({ message: 'Failed to persist quick practice item.' });
  }
});

app.delete(`${QUICK_PRACTICE_API_BASE}/:id`, async (req, res) => {
  const { id } = req.params;
  const index = quickPracticeItems.findIndex((item) => item.id === id);

  if (index < 0) {
    res.status(404).json({ message: 'Quick practice item not found.' });
    return;
  }

  const [removed] = quickPracticeItems.splice(index, 1);

  try {
    await queueQuickPracticeWrite();
    res.json({ deletedId: removed.id });
  } catch (_error) {
    quickPracticeItems.splice(index, 0, removed);
    res.status(500).json({ message: 'Failed to delete quick practice item.' });
  }
});

// Serve static files from the dist directory
app.use(express.static(path.join(__dirname, 'dist'), {
  // Set proper MIME types
  setHeaders: (res, filePath) => {
    if (filePath.endsWith('.m4a')) {
      res.setHeader('Content-Type', 'audio/mp4');
    }
    // Enable caching for static assets
    if (filePath.match(/\.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$/)) {
      res.setHeader('Cache-Control', 'public, max-age=31536000, immutable');
    }
    // Cache audio and JSON files
    if (filePath.match(/\.(m4a|json)$/)) {
      res.setHeader('Cache-Control', 'public, max-age=31536000, immutable');
    }
  }
}));

// SPA routing - serve index.html for all non-static routes
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist', 'index.html'));
});

loadQuickPracticeStore().then(() => {
  app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
  console.log(`Serving static files from: ${path.join(__dirname, 'dist')}`);
  });
}).catch((error) => {
  console.error(`Server startup failed: ${error.message}`);
  process.exit(1);
});
