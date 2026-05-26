const buckets = new Map();
const SAVE_MAX_PER_WINDOW = 30;
const SAVE_WINDOW_MS = 60_000;
export async function rateLimitSave(request, reply) {
    const playerId = request.auth?.playerId;
    if (!playerId) {
        return;
    }
    const now = Date.now();
    const bucket = buckets.get(playerId);
    if (!bucket || now >= bucket.resetAt) {
        buckets.set(playerId, { count: 1, resetAt: now + SAVE_WINDOW_MS });
        return;
    }
    bucket.count += 1;
    if (bucket.count > SAVE_MAX_PER_WINDOW) {
        return reply.code(429).send({ error: "Too many save requests. Try again shortly." });
    }
}
