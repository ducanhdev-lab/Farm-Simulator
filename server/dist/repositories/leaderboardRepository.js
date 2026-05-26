import { pool } from "../config/database.js";
export async function upsertLeaderboardScore(playerId, coins, saveVersion) {
    await pool.query(`INSERT INTO leaderboard_scores (player_id, display_coins, save_version, updated_at)
     VALUES ($1, $2, $3, NOW())
     ON CONFLICT (player_id) DO UPDATE SET
       display_coins = EXCLUDED.display_coins,
       save_version = EXCLUDED.save_version,
       updated_at = NOW()
     WHERE leaderboard_scores.display_coins <= EXCLUDED.display_coins`, [playerId, coins, saveVersion]);
}
export async function getTopCoinScores(limit) {
    const capped = Math.min(Math.max(limit, 1), 100);
    const result = await pool.query(`SELECT player_id, display_coins, save_version, updated_at
     FROM leaderboard_scores
     ORDER BY display_coins DESC, updated_at ASC
     LIMIT $1`, [capped]);
    return result.rows;
}
