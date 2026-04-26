# Quick Practice Implementation Stories (Phased)

This document converts `docs/requirements.md` into phased delivery stories so implementation can happen in small, reviewable increments.

## Phase 0 - Setup and Contracts

## Story 0.1: Confirm API and data contracts
**Goal**  
Lock the request/response shape for Quick Practice before implementation.

**Scope**
- Finalize item model: `id`, `chapterNumber`, `slokaNumber`, `createdAt`.
- Finalize endpoint contracts:
  - `GET /api/quick-practice`
  - `POST /api/quick-practice`
  - `DELETE /api/quick-practice/:id` (if included in V1)
- Finalize validation rules for chapter/sloka.

**Acceptance Criteria**
- API contract is documented and consistent across frontend/backend tasks.
- Validation limits are explicitly listed: chapter `0..18`, sloka `>= 1`.

---

## Phase 1 - Backend Persistence and API

## Story 1.1: Add Quick Practice file store
**Goal**  
Persist quick-practice data in a simple global JSON file.

**Scope**
- Create server-side storage file (for example `data/quick-practice.json`).
- Load existing data at server start.
- Persist writes on create/delete operations.
- Serialize writes in-process to avoid corruption.

**Acceptance Criteria**
- Data survives server restart in non-ephemeral environments.
- Invalid storage file content does not crash server startup; safe fallback is applied.

## Story 1.2: Implement Quick Practice API endpoints
**Goal**  
Expose CRUD-lite APIs for frontend integration.

**Scope**
- Implement:
  - `GET /api/quick-practice`
  - `POST /api/quick-practice`
  - Optional `DELETE /api/quick-practice/:id`
- Add backend validation and error responses (`400`, `500`).

**Acceptance Criteria**
- `POST` with valid data creates and returns item with backend-generated `id`.
- `POST` with invalid data returns `400` with clear message.
- `GET` returns items in insertion order.
- If `DELETE` is implemented, deleting an existing `id` removes item and persists change.

---

## Phase 2 - Navigation and Page Shell

## Story 2.1: Add Quick Practice navigation link
**Goal**  
Make Quick Practice discoverable in main app navigation.

**Scope**
- Add `Quick Practice` tab/button in side navigation.
- Add top-level app state and route/tab handling to render Quick Practice page.

**Acceptance Criteria**
- `Quick Practice` appears with existing practice modes.
- Clicking it renders Quick Practice screen without breaking existing tabs.

## Story 2.2: Build Quick Practice page skeleton
**Goal**  
Create the screen structure for add/list/practice sections.

**Scope**
- Add section (chapter, sloka, add button).
- Saved list section placeholder.
- Practice section with `Play`, current-item area, `Done`.
- Empty and loading placeholders.

**Acceptance Criteria**
- Screen has all three sections and consistent styling with app.
- `Play` is disabled when no items are available.

---

## Phase 3 - Add and List Features

## Story 3.1: Implement add item flow (UI to API)
**Goal**  
Allow users to add chapter+sloka to global quick list.

**Scope**
- Wire add form submission to `POST /api/quick-practice`.
- Validate inputs on frontend before submit.
- Show backend validation messages.

**Acceptance Criteria**
- Valid input adds item and updates list in UI.
- Invalid chapter/sloka shows clear inline error.
- API/network failure shows non-blocking error message.

## Story 3.2: Implement list fetch/render flow
**Goal**  
Display existing quick-practice items from backend.

**Scope**
- Fetch list on Quick Practice page load.
- Render items as `Chapter X - Sloka Y`.
- Show empty-state text when list is empty.

**Acceptance Criteria**
- Existing items appear on load in insertion order.
- Empty state is shown when no items are returned.

## Story 3.3 (Optional V1): Remove item
**Goal**  
Enable manual cleanup of quick-practice list.

**Scope**
- Add per-item remove action in UI.
- Wire action to `DELETE /api/quick-practice/:id`.

**Acceptance Criteria**
- Removing an item updates UI and backend persistence.

---

## Phase 4 - Practice Playback Flow

## Story 4.1: Implement Quick Practice state machine
**Goal**  
Enable self-paced sequential practice with `Done` transitions.

**Scope**
- Practice states: idle, playingPrompt, waitingForDone, replaying, advancing, completed.
- Start from first item on `Play`.
- Move to next item only after `Done` cycle.

**Acceptance Criteria**
- `Play` starts sequence from first item.
- `Done` triggers replay and advances to next.
- End-of-list completion state is shown with restart option.

## Story 4.2: Integrate sloka audio targeting
**Goal**  
Play the correct chapter/sloka audio segment for each quick-practice item.

**Scope**
- Reuse existing metadata/audio mechanisms.
- Resolve chapter and sloka references for each item.
- Handle missing metadata/audio safely.

**Acceptance Criteria**
- Correct sloka audio segment is played for current item.
- Missing or invalid content is surfaced as user-visible error and sequence can continue safely.

---

## Phase 5 - Hardening and Test Coverage

## Story 5.1: Validation hardening
**Goal**  
Ensure consistent validation behavior across UI and API.

**Scope**
- Align frontend/backend validation messages and boundaries.
- Validate integer-only inputs and required fields.

**Acceptance Criteria**
- All invalid input paths return deterministic errors.
- Frontend and backend rules are consistent.

## Story 5.2: Basic test coverage
**Goal**  
Add minimum confidence tests for critical flows.

**Scope**
- Backend tests: `GET`, `POST` validation, optional `DELETE`.
- Frontend tests: add flow, empty state, play/done transition basics.
- Smoke check existing modes remain functional.

**Acceptance Criteria**
- Critical Quick Practice flows have automated coverage.
- No regressions in existing Individual/Group mode entry paths.

---

## Delivery Notes

- Keep implementation intentionally simple and global.
- Do not add auth or user profiles.
- Duplicates are allowed in V1.
- If runtime host is ephemeral, file-based persistence reset risk should be documented in release notes.
