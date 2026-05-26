import Fastify from "fastify";
import cors from "@fastify/cors";
import { env } from "./config/env.js";
import { initDatabase } from "./config/database.js";
import { seedLiveEventsIfEmpty } from "./repositories/eventsRepository.js";
import { seedDemoLeaderboard } from "./repositories/leaderboardSeedRepository.js";
import { devRoutes } from "./routes/devRoutes.js";
import { authRoutes } from "./routes/authRoutes.js";
import { playerRoutes } from "./routes/playerRoutes.js";
import { leaderboardRoutes } from "./routes/leaderboardRoutes.js";
import { configRoutes } from "./routes/configRoutes.js";
import { eventsRoutes } from "./routes/eventsRoutes.js";
import { snapshotRoutes } from "./routes/snapshotRoutes.js";
const app = Fastify({ logger: true });
await app.register(cors, { origin: true });
await initDatabase();
await seedLiveEventsIfEmpty();
const demoLeaderboardCount = await seedDemoLeaderboard();
if (demoLeaderboardCount > 0) {
    app.log.info(`Demo leaderboard: ${demoLeaderboardCount} entries ready for testing.`);
}
app.get("/", async () => ({
    name: "Farm Life API",
    status: "ok",
    endpoints: {
        health: "GET /health",
        guestAuth: "POST /auth/guest",
        profile: "GET /player/profile (Bearer JWT)",
        save: "PUT /player/save (Bearer JWT)",
        leaderboard: "GET /leaderboard/coins",
        remoteConfig: "GET /config/balance",
        liveEvents: "GET /events/active",
        islandSnapshot: "PUT /island/snapshot, GET /island/snapshot/:playerId",
    },
}));
app.get("/health", async () => ({ status: "ok" }));
await app.register(authRoutes);
await app.register(configRoutes);
await app.register(eventsRoutes);
await app.register(leaderboardRoutes);
await app.register(playerRoutes);
await app.register(snapshotRoutes);
await app.register(devRoutes);
app.listen({ port: env.port, host: "0.0.0.0" });
