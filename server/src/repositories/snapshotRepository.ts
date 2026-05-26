import { pool } from "../config/database.js";

export interface SnapshotRow {
  player_id: string;
  snapshot_json: Record<string, unknown>;
  updated_at: string;
}

export async function upsertIslandSnapshot(
  playerId: string,
  snapshot: Record<string, unknown>
): Promise<void> {
  await pool.query(
    `INSERT INTO island_snapshots (player_id, snapshot_json, updated_at)
     VALUES ($1, $2::jsonb, NOW())
     ON CONFLICT (player_id) DO UPDATE SET
       snapshot_json = EXCLUDED.snapshot_json,
       updated_at = NOW()`,
    [playerId, JSON.stringify(snapshot)]
  );
}

export async function getIslandSnapshot(playerId: string): Promise<SnapshotRow | null> {
  const result = await pool.query<SnapshotRow>(
    `SELECT player_id, snapshot_json, updated_at
     FROM island_snapshots
     WHERE player_id = $1`,
    [playerId]
  );

  return result.rows[0] ?? null;
}
