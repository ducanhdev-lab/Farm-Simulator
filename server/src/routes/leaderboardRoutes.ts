import type { FastifyInstance } from "fastify";
import { getCoinLeaderboard } from "../controllers/leaderboardController.js";

export async function leaderboardRoutes(app: FastifyInstance) {
  app.get("/leaderboard/coins", getCoinLeaderboard);
}
