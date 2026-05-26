import { pool } from "../config/database.js";

export async function upsertPlayerProfile(
  playerId: string,
  saveVersion: number,
  profileJson: Record<string, unknown>
): Promise<{ ok: boolean; conflict?: boolean }> {
  const existing = await pool.query<{ save_version: number }>(
    "SELECT save_version FROM players WHERE id = $1",
    [playerId]
  );

  if (existing.rowCount && existing.rows[0].save_version > saveVersion) {
    return { ok: false, conflict: true };
  }

  await pool.query(
    `INSERT INTO players (id, save_version, profile_json, updated_at)
     VALUES ($1, $2, $3::jsonb, NOW())
     ON CONFLICT (id) DO UPDATE SET
       save_version = EXCLUDED.save_version,
       profile_json = EXCLUDED.profile_json,
       updated_at = NOW()
     WHERE players.save_version <= EXCLUDED.save_version`,
    [playerId, saveVersion, JSON.stringify(profileJson)]
  );

  return { ok: true };
}

export async function getPlayerProfile(playerId: string) {
  const result = await pool.query<{
    id: string;
    save_version: number;
    profile_json: Record<string, unknown>;
  }>("SELECT id, save_version, profile_json FROM players WHERE id = $1", [playerId]);

  return result.rows[0] ?? null;
}

export async function createPlayer(playerId: string) {
  await pool.query(
    `INSERT INTO players (id, save_version, profile_json)
     VALUES ($1, 1, '{}'::jsonb)
     ON CONFLICT (id) DO NOTHING`,
    [playerId]
  );
}
