const MAX_COINS = 50_000_000;
const MAX_ISLANDS = 10_000;

export interface SnapshotValidationResult {
  valid: boolean;
  reason?: string;
}

export function validateIslandSnapshot(snapshot: Record<string, unknown>): SnapshotValidationResult {
  const coins = snapshot.coins;
  if (typeof coins === "number" && (coins < 0 || coins > MAX_COINS)) {
    return { valid: false, reason: "Invalid snapshot coins" };
  }

  const islands = snapshot.unlockedIslandCount;
  if (typeof islands === "number" && (islands < 0 || islands > MAX_ISLANDS)) {
    return { valid: false, reason: "Invalid unlocked island count" };
  }

  return { valid: true };
}
