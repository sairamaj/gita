# CLAUDE.md

This file provides guidance for Claude or any coding agent working in this repository.

## Project Overview

`gita` contains two primary application surfaces:

- `src/Gita.Practice.App`: WPF desktop application (.NET/C#).
- `src/web/client`: React + Vite web application with an Express server entrypoint (`server.js`).

There are CI/CD workflows under `.github/workflows`, including deployment for the web app to Azure.

## Repository Structure

- `Gita.Practice.App.sln`: Root Visual Studio solution.
- `src/Gita.Practice.App/`: Desktop app code (views, viewmodels, models, repository/services).
- `src/web/`: Web docs and deployment references.
- `src/web/client/`: Web runtime project (`package.json`, `vite.config.js`, React source, public data).

## Local Development

### Web app (`src/web/client`)

Run from `src/web/client`:

- Install dependencies: `npm ci`
- Start dev server: `npm run dev`
- Build production assets: `npm run build`
- Preview build: `npm run preview`
- Start Express server: `npm start`

### Desktop app (`src/Gita.Practice.App`)

Open `Gita.Practice.App.sln` in Visual Studio and run/build the WPF app.

When using CLI builds on Windows with .NET SDK installed:

- Restore/build solution: `dotnet build Gita.Practice.App.sln`

## CI/CD Notes

- Main workflow: `.github/workflows/main_gita-practice.yml`
- Web CI build uses Node 20 and builds from `src/web/client`.
- Deployment package includes `dist`, `server.js`, and `package*.json`.
- Azure deployment uses secrets configured in GitHub Actions.

## Coding Guidelines for Agents

- Keep changes scoped to the user request; avoid broad refactors.
- Prefer minimal, targeted edits over architectural rewrites.
- Do not commit secrets or modify deployment credentials.
- If editing workflows, preserve existing app name/slot and secret wiring unless explicitly requested.
- For frontend changes, ensure build passes with `npm run build`.
- For desktop changes, keep MVVM boundaries intact (ViewModels for state/logic, Views for XAML/UI binding).

## Validation Checklist

Before finalizing substantive changes:

- For web edits: run `npm run build` in `src/web/client`.
- For desktop edits: run solution build when possible (`dotnet build Gita.Practice.App.sln`).
- Confirm no unrelated file changes are introduced.

## Documentation Conventions

- Put new web-specific docs under `src/web/`.
- Keep root-level docs focused on cross-project context.
- Update this file if project structure or primary workflows change.
