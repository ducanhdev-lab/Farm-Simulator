import { pool } from "../config/database.js";

export interface LeaderboardRow {
  player_id: string;
  display_coins: number;
  save_version: number;
  updated_at: string;
  display_name: string | null;
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
    `SELECT ls.player_id, ls.display_coins, ls.save_version, ls.updated_at,
            COALESCE(
              NULLIF(p.profile_json->>'displayName', ''),
              NULLIF(p.profile_json->>'display_name', '')
            ) AS display_name
     FROM leaderboard_scores ls
     LEFT JOIN players p ON p.id = ls.player_id
     ORDER BY ls.display_coins DESC, ls.updated_at ASC
     LIMIT $1`,
    [capped]
  );
  return result.rows;
}
