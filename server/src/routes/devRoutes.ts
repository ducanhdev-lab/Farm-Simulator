import type { FastifyInstance } from "fastify";
import {
  isDemoLeaderboardSeedingEnabled,
  seedDemoLeaderboard,
} from "../repositories/leaderboardSeedRepository.js";

export async function devRoutes(app: FastifyInstance) {
  app.post("/dev/seed-leaderboard", async (_request, reply) => {
    if (!isDemoLeaderboardSeedingEnabled()) {
      return reply.code(403).send({
        error: "Demo leaderboard seeding is disabled. Set SEED_DEMO_LEADERBOARD=true or use non-production NODE_ENV.",
      });
    }

    const count = await seedDemoLeaderboard();
    return reply.send({ ok: true, seeded: count, message: `${count} demo leaderboard entries upserted.` });
  });
}
