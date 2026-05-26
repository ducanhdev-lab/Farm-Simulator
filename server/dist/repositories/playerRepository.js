import { pool } from "../config/database.js";
export async function upsertPlayerProfile(playerId, saveVersion, profileJson) {
    const existing = await pool.query("SELECT save_version FROM players WHERE id = $1", [playerId]);
    if (existing.rowCount && existing.rows[0].save_version > saveVersion) {
        return { ok: false, conflict: true };
    }
    await pool.query(`INSERT INTO players (id, save_version, profile_json, updated_at)
     VALUES ($1, $2, $3::jsonb, NOW())
     ON CONFLICT (id) DO UPDATE SET
       save_version = EXCLUDED.save_version,
       profile_json = EXCLUDED.profile_json,
       updated_at = NOW()
     WHERE players.save_version <= EXCLUDED.save_version`, [playerId, saveVersion, JSON.stringify(profileJson)]);
    return { ok: true };
}
export async function getPlayerProfile(playerId) {
    const result = await pool.query("SELECT id, save_version, profile_json, updated_at FROM players WHERE id = $1", [playerId]);
    return result.rows[0] ?? null;
}
export async function createPlayer(playerId) {
    await pool.query(`INSERT INTO players (id, save_version, profile_json)
     VALUES ($1, 1, '{}'::jsonb)
     ON CONFLICT (id) DO NOTHING`, [playerId]);
}
