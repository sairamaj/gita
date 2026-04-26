# Quick Practice API Contract (Phase 0)

This contract locks the request/response shape for Quick Practice V1.

## Data Model

Each item in the quick-practice list must match:

```json
{
  "id": "string",
  "chapterNumber": 0,
  "slokaNumber": 1,
  "createdAt": "2026-04-25T19:15:00.000Z"
}
```

Validation rules:

- `chapterNumber`: integer in `0..18`.
- `slokaNumber`: integer `>= 1`.
- `id`: backend-generated unique string.
- `createdAt`: backend-generated ISO-8601 timestamp string.

Order rules:

- Read APIs return insertion order (`createdAt` ascending).

Duplicate rules:

- Duplicates are allowed in V1.

## Endpoints

### GET `/api/quick-practice`

Returns all items in insertion order.

Success `200`:

```json
{
  "quickPracticeItems": [
    {
      "id": "qp_1",
      "chapterNumber": 2,
      "slokaNumber": 47,
      "createdAt": "2026-04-25T19:15:00.000Z"
    }
  ]
}
```

Failure `500`:

```json
{
  "message": "Failed to read quick practice items."
}
```

### POST `/api/quick-practice`

Creates one item.

Request body:

```json
{
  "chapterNumber": 2,
  "slokaNumber": 47
}
```

Success `201`:

```json
{
  "item": {
    "id": "qp_2",
    "chapterNumber": 2,
    "slokaNumber": 47,
    "createdAt": "2026-04-25T19:16:00.000Z"
  }
}
```

Validation failure `400`:

```json
{
  "message": "chapterNumber must be between 0 and 18."
}
```

Storage failure `500`:

```json
{
  "message": "Failed to persist quick practice item."
}
```

### DELETE `/api/quick-practice/:id` (Optional in V1)

Removes one item by `id`.

Success `200`:

```json
{
  "deletedId": "qp_2"
}
```

Not found `404`:

```json
{
  "message": "Quick practice item not found."
}
```

Storage failure `500`:

```json
{
  "message": "Failed to delete quick practice item."
}
```

## Shared Contract Source

The canonical constants and validation helpers are defined in:

- `src/web/client/src/contracts/quickPracticeContracts.js`

Phase 1 backend and Phase 3 frontend integration should use that module to keep rules/messages aligned.
