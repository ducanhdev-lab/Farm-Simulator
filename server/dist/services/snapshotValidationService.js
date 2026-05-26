const MAX_COINS = 999_999_999;
const MAX_ISLANDS = 10_000;
export function validateIslandSnapshot(snapshot) {
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
