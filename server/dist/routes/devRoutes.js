import { isDemoLeaderboardSeedingEnabled, seedDemoLeaderboard, } from "../repositories/leaderboardSeedRepository.js";
import { listAllLiveEvents, updateLiveEvent, } from "../repositories/eventsRepository.js";
function isDevToolsEnabled() {
    if (process.env.NODE_ENV === "production" && process.env.ENABLE_DEV_ROUTES !== "true") {
        return false;
    }
    return true;
}
function devForbidden(reply) {
    return reply.code(403).send({
        error: "Dev routes disabled. Set ENABLE_DEV_ROUTES=true in production or use non-production NODE_ENV.",
    });
}
export async function devRoutes(app) {
    app.post("/dev/seed-leaderboard", async (_request, reply) => {
        if (!isDevToolsEnabled()) {
            return devForbidden(reply);
        }
        if (!isDemoLeaderboardSeedingEnabled()) {
            return reply.code(403).send({
                error: "Demo leaderboard seeding is disabled. Set SEED_DEMO_LEADERBOARD=true or use non-production NODE_ENV.",
            });
        }
        const count = await seedDemoLeaderboard();
        return reply.send({ ok: true, seeded: count, message: `${count} demo leaderboard entries upserted.` });
    });
    app.get("/dev/events", async (_request, reply) => {
        if (!isDevToolsEnabled()) {
            return devForbidden(reply);
        }
        const events = await listAllLiveEvents();
        return reply.send({ events });
    });
    app.put("/dev/events/:eventId", async (request, reply) => {
        if (!isDevToolsEnabled()) {
            return devForbidden(reply);
        }
        const { eventId } = request.params;
        const body = request.body;
        const updated = await updateLiveEvent(eventId, {
            sceneName: body.sceneName,
            displayName: body.displayName,
            description: body.description,
            alwaysAvailable: body.alwaysAvailable,
            requiredHomeCoins: body.requiredHomeCoins,
            startsAt: body.startsAt,
            endsAt: body.endsAt,
            isEnabled: body.isEnabled,
        });
        if (!updated) {
            return reply.code(404).send({ error: "Event not found" });
        }
        return reply.send({ event: updated });
    });
}
