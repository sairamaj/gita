# Quick Practice Requirements

## 1) Purpose

The app currently supports practice across all 19 chapters (0 through 18).  
Some slokas need frequent repetition, so users need a fast way to save and re-practice selected chapter+sloka pairs.

This document defines requirements for a new web app feature named **Quick Practice**.

## 2) Scope

### In scope
- Add a new left navigation link/tab: `Quick Practice`.
- Allow adding an item using:
  - `chapterNumber` (integer, 0..18)
  - `slokaNumber` (integer, >= 1)
- Show list of already added quick-practice items.
- Provide a sequential practice flow with a play control and manual recitation pacing.
- Persist quick-practice items in backend storage.
- Storage is global/shared for all users (no user identity split).

### Out of scope
- Per-user quick-practice lists.
- Authentication/authorization.
- Spaced-repetition algorithms, streaks, scheduling intelligence.
- Cross-device profile sync.

## 3) User Stories

- As a practitioner, I can add a chapter+sloka item to Quick Practice so I can revisit difficult slokas quickly.
- As a practitioner, I can view all saved Quick Practice items so I know what is in my quick list.
- As a practitioner, I can run Quick Practice in sequence with my own recitation timing (Done button) so practice remains self-paced.
- As a practitioner, I can use the same shared list as everyone else so setup remains simple.

## 4) UI/UX Requirements

## 4.1 Navigation
- Add a new nav action/button labeled `Quick Practice` alongside existing modes.
- Selecting it loads the Quick Practice screen in the main content area.

## 4.2 Quick Practice Screen Layout
- **Add section**
  - Input/select for chapter number (0..18).
  - Input for sloka number (positive integer).
  - `Add` button.
  - Validation and error messaging near inputs.
- **Saved items section**
  - List each saved item as `Chapter X - Sloka Y`.
  - Optional remove action can be deferred unless implementation finds it necessary.
- **Practice section**
  - `Play` button to start practicing from first item in current order.
  - Current item display (chapter and sloka).
  - `Done` button for user to signal "I finished reciting."
  - On `Done`, system replays target sloka (or advances according to practice state) and then moves to next item.
  - End-of-list state shown clearly with option to restart.

## 4.3 Practice Flow Behavior
- Initial state: waiting for user to press `Play`.
- When started:
  1. System plays current target sloka audio segment.
  2. System waits for user self-recitation (no auto-timeout required for V1).
  3. User clicks `Done`.
  4. System plays target again for reinforcement.
  5. System advances to next item and repeats.
- If no items exist, `Play` is disabled and an empty-state message is shown.

## 5) Data Model Requirements

Use a simple global model:

```json
{
  "quickPracticeItems": [
    {
      "id": "string",
      "chapterNumber": 0,
      "slokaNumber": 1,
      "createdAt": "ISO-8601 string"
    }
  ]
}
```

Constraints:
- `chapterNumber`: integer in `[0, 18]`.
- `slokaNumber`: integer `>= 1`.
- `id`: backend-generated unique value (string).

Duplicate policy (simple default):
- Allow duplicates in V1 to keep logic minimal.
- Optional dedupe can be introduced later as enhancement.

Ordering:
- Default order is insertion order (`createdAt` ascending).

## 6) Backend Requirements (Global Persistence)

Current web app uses an Express server entrypoint.  
For simplicity, add lightweight backend storage in server runtime.

Recommended V1 storage:
- JSON file persisted on server filesystem (for example: `data/quick-practice.json` under server working directory).
- Load at server start; write file on every create/update operation.

Important environment note:
- Local/dev and non-ephemeral hosts preserve file changes.
- On ephemeral or redeploy-reset environments, file-based data may reset unless external persistent volume is configured.

No database required for V1.

## 7) API Requirements

Minimum endpoints:

- `GET /api/quick-practice`
  - Returns all items in practice order.
- `POST /api/quick-practice`
  - Body: `{ "chapterNumber": number, "slokaNumber": number }`
  - Validates ranges and type.
  - Returns created item.
- `DELETE /api/quick-practice/:id` (optional but recommended for manageability)
  - Removes one item.
  - If omitted in V1, keep list append-only and document this in UI.

Response/error behavior:
- `400` for validation errors.
- `500` for storage read/write failures.
- JSON response body with clear `message`.

## 8) Validation and Error Handling

Frontend and backend must both validate:
- Chapter outside 0..18 -> reject with clear message.
- Sloka <= 0 or non-integer -> reject.
- Missing fields -> reject.

UI error states:
- Inline form error on invalid input.
- Non-blocking banner/toast for API failures.
- Empty-state text when list has no items.

## 9) Non-Functional Requirements

- Keep implementation simple and maintainable.
- Do not add user login or new infrastructure dependencies.
- Ensure API operations are fast for small list sizes (< 1000 items).
- Avoid data corruption on concurrent writes by serializing file writes in process.

## 10) Acceptance Criteria (Story Ready)

### AC1: Navigation
- Given the app is loaded
- When user views practice mode navigation
- Then user sees `Quick Practice` link
- And selecting it opens Quick Practice screen.

### AC2: Add Item
- Given user is on Quick Practice screen
- When user submits valid chapter and sloka
- Then item is persisted by backend
- And item appears in saved list after refresh.

### AC3: Validation
- Given user enters invalid chapter/sloka
- When user submits form
- Then submission is rejected
- And clear validation message is shown.

### AC4: View Saved List
- Given backend has existing quick-practice items
- When Quick Practice screen loads
- Then app fetches and displays all saved items in insertion order.

### AC5: Practice Sequence
- Given saved list has multiple items
- When user clicks `Play`
- Then first item starts
- And app waits for user recitation
- When user clicks `Done`
- Then app plays reinforcement for current item
- And advances to next item
- And repeats until list completion.

### AC6: Empty State
- Given there are no saved items
- When user opens Quick Practice
- Then user sees empty-state guidance
- And `Play` is disabled.

### AC7: Global Data
- Given two different users/browsers open the app
- When one adds an item
- Then the item is part of the same global quick-practice list
- And becomes visible to others after refresh.

## 11) Implementation Notes for Story Breakdown

Suggested story split:
- Story A: Add nav + Quick Practice page shell.
- Story B: Backend file store + API endpoints.
- Story C: Add/list UI wired to API.
- Story D: Practice flow state machine (`Play` -> wait -> `Done` -> replay -> next).
- Story E: Validation + error states + basic tests.
