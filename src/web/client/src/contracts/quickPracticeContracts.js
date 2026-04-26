export const QUICK_PRACTICE_API_BASE = "/api/quick-practice";

export const QUICK_PRACTICE_LIMITS = {
  chapterNumber: {
    min: 0,
    max: 18,
  },
  slokaNumber: {
    min: 1,
  },
};

/**
 * Canonical Quick Practice item contract shared across frontend/backend tasks.
 * @typedef {Object} QuickPracticeItem
 * @property {string} id
 * @property {number} chapterNumber
 * @property {number} slokaNumber
 * @property {string} createdAt
 */

function isInteger(value) {
  return Number.isInteger(value);
}

function hasOwn(obj, key) {
  return Object.prototype.hasOwnProperty.call(obj, key);
}

export function validateQuickPracticeCreatePayload(payload) {
  if (!payload || typeof payload !== "object") {
    return { valid: false, message: "Request body must be a JSON object." };
  }

  if (!hasOwn(payload, "chapterNumber")) {
    return { valid: false, message: "chapterNumber is required." };
  }

  if (!hasOwn(payload, "slokaNumber")) {
    return { valid: false, message: "slokaNumber is required." };
  }

  const { chapterNumber, slokaNumber } = payload;

  if (!isInteger(chapterNumber)) {
    return { valid: false, message: "chapterNumber must be an integer." };
  }

  if (
    chapterNumber < QUICK_PRACTICE_LIMITS.chapterNumber.min ||
    chapterNumber > QUICK_PRACTICE_LIMITS.chapterNumber.max
  ) {
    return {
      valid: false,
      message: `chapterNumber must be between ${QUICK_PRACTICE_LIMITS.chapterNumber.min} and ${QUICK_PRACTICE_LIMITS.chapterNumber.max}.`,
    };
  }

  if (!isInteger(slokaNumber)) {
    return { valid: false, message: "slokaNumber must be an integer." };
  }

  if (slokaNumber < QUICK_PRACTICE_LIMITS.slokaNumber.min) {
    return {
      valid: false,
      message: `slokaNumber must be >= ${QUICK_PRACTICE_LIMITS.slokaNumber.min}.`,
    };
  }

  return { valid: true };
}

export function validateQuickPracticeItem(item) {
  if (!item || typeof item !== "object") {
    return { valid: false, message: "Item must be an object." };
  }

  if (typeof item.id !== "string" || item.id.trim() === "") {
    return { valid: false, message: "Item id must be a non-empty string." };
  }

  if (typeof item.createdAt !== "string" || item.createdAt.trim() === "") {
    return { valid: false, message: "Item createdAt must be an ISO-8601 string." };
  }

  return validateQuickPracticeCreatePayload(item);
}
