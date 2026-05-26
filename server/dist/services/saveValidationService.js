/** Max home coins accepted on cloud save / leaderboard (solo farm can exceed 50M). */
const MAX_COINS = 999_999_999;
/** Below this cloud balance, treat next upload as offline catch-up (no per-minute delta cap). */
const OFFLINE_CATCHUP_THRESHOLD = 50_000;
const MAX_SAVE_VERSION = 10_000;
const MAX_COIN_INCREASE_PER_SAVE = 15_000;
const MAX_COIN_PER_MINUTE = 800;
const MIN_ELAPSED_MINUTES = 0.5;
export function extractCoins(profile) {
    if (!profile) {
        return 0;
    }
    const homeWorld = profile.homeWorld;
    const coins = homeWorld?.Coin;
    return typeof coins === "number" && Number.isFinite(coins) ? Math.max(0, Math.floor(coins)) : 0;
}
function elapsedMinutes(from, now) {
    if (!from) {
        return 1;
    }
    return Math.max((now.getTime() - from.getTime()) / 60_000, MIN_ELAPSED_MINUTES);
}
export function validateCoinDelta(previousCoins, newCoins, context, now = new Date()) {
    if (newCoins < previousCoins) {
        return { valid: true, coins: newCoins };
    }
    const delta = newCoins - previousCoins;
    if (delta === 0) {
        return { valid: true, coins: newCoins };
    }
    if (delta > MAX_COIN_INCREASE_PER_SAVE) {
        return { valid: false, reason: "Coin increase exceeds per-save limit" };
    }
    const minutes = elapsedMinutes(context?.previousUpdatedAt, now);
    const allowedBySession = Math.ceil(minutes) * MAX_COIN_PER_MINUTE + 500;
    if (delta > allowedBySession) {
        return { valid: false, reason: "Coin increase exceeds session rate limit" };
    }
    return { valid: true, coins: newCoins };
}
export function validatePlayerProfile(profile, context) {
    if (!profile || typeof profile !== "object") {
        return { valid: false, reason: "Profile must be an object" };
    }
    const saveVersion = profile.saveVersion;
    if (typeof saveVersion === "number" && (saveVersion < 0 || saveVersion > MAX_SAVE_VERSION)) {
        return { valid: false, reason: "Invalid saveVersion" };
    }
    const homeWorld = profile.homeWorld;
    const coins = homeWorld?.Coin;
    if (typeof coins !== "number") {
        return { valid: true, coins: 0 };
    }
    if (!Number.isFinite(coins) || coins < 0 || coins > MAX_COINS) {
        return { valid: false, reason: "Invalid coin amount" };
    }
    const newCoins = Math.floor(coins);
    const previousCoins = extractCoins(context?.previousProfile ?? null);
    if (newCoins <= MAX_COINS && previousCoins < OFFLINE_CATCHUP_THRESHOLD) {
        return { valid: true, coins: newCoins };
    }
    return validateCoinDelta(previousCoins, newCoins, context);
}
