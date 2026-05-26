import { pool } from "../config/database.js";

export interface LeaderboardRow {
  player_id: string;
  display_coins: number;
  save_version: number;
  updated_at: string;
}

export async function upsertLeaderboardScore(
  playerId: string,
  coins: number,
  saveVersion: number
): Promise<void> {
  await pool.query(
    `INSERT INTO leaderboard_scores (player_id, display_coins, save_version, updated_at)
     VALUES ($1, $2, $3, NOW())
     ON CONFLICT (player_id) DO UPDATE SET
       display_coins = EXCLUDED.display_coins,
       save_version = EXCLUDED.save_version,
       updated_at = NOW()
     WHERE leaderboard_scores.display_coins <= EXCLUDED.display_coins`,
    [playerId, coins, saveVersion]
  );
}

export async function getTopCoinScores(limit: number): Promise<LeaderboardRow[]> {
  const capped = Math.min(Math.max(limit, 1), 100);
  const result = await pool.query<LeaderboardRow>(
    `SELECT player_id, display_coins, save_version, updated_at
     FROM leaderboard_scores
     ORDER BY display_coins DESC, updated_at ASC
     LIMIT $1`,
    [capped]
  );
  return result.rows;
}
