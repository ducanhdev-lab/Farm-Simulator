import { getTopCoinScores } from "../repositories/leaderboardRepository.js";
export async function getCoinLeaderboard(request, reply) {
    const query = request.query;
    const limit = query.limit ? Number.parseInt(query.limit, 10) : 20;
    const rows = await getTopCoinScores(Number.isNaN(limit) ? 20 : limit);
    return reply.send({
        entries: rows.map((row, index) => ({
            rank: index + 1,
            playerId: row.player_id,
            displayName: row.display_name ?? undefined,
            coins: row.display_coins,
            saveVersion: row.save_version,
            updatedAt: row.updated_at,
        })),
    });
}
