import type { FastifyReply, FastifyRequest } from "fastify";
import { getTopCoinScores } from "../repositories/leaderboardRepository.js";

export async function getCoinLeaderboard(request: FastifyRequest, reply: FastifyReply) {
  const query = request.query as { limit?: string };
  const limit = query.limit ? Number.parseInt(query.limit, 10) : 20;

  const rows = await getTopCoinScores(Number.isNaN(limit) ? 20 : limit);

  return reply.send({
    entries: rows.map((row, index) => ({
      rank: index + 1,
      playerId: row.player_id,
      coins: row.display_coins,
      saveVersion: row.save_version,
      updatedAt: row.updated_at,
    })),
  });
}
