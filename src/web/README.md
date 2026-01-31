# Gita Practice Web

This folder contains a React UI and a Flask API that mirror the WPF app.

## Quick Start

### Backend (Flask)

1. Create a virtual environment and install dependencies:

```
cd src/web/server
python -m venv .venv
.\.venv\Scripts\activate
pip install -r requirements.txt
```

2. Run the API:

```
python app.py
```

The API serves chapter metadata and audio from `https://www.sgsgitafoundation.org/bg/`,
with local caching under `src/web/server/data`.

### Frontend (React)

1. Install dependencies and start the dev server:

```
cd src/web/client
npm install
npm run dev
```

2. Open the URL shown by Vite (usually `http://localhost:5173`).

## API Endpoints

- `GET /api/health`
- `GET /api/config`
- `GET /api/chapters`
- `GET /api/chapters/{chapterId}/metadata`
- `GET /api/chapters/{chapterId}/audio`
