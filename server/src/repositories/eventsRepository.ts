import { pool } from "../config/database.js";

export interface LiveEventRow {
  event_id: string;
  scene_name: string;
  display_name: string;
  description: string;
  always_available: boolean;
  required_home_coins: number;
  starts_at: string | null;
  ends_at: string | null;
  is_enabled: boolean;
}

export async function getActiveEventsFromDb(now = new Date()): Promise<LiveEventRow[]> {
  const result = await pool.query<LiveEventRow>(
    `SELECT event_id, scene_name, display_name, description,
            always_available, required_home_coins, starts_at, ends_at, is_enabled
     FROM live_events
     WHERE is_enabled = true
       AND (starts_at IS NULL OR starts_at <= $1)
       AND (ends_at IS NULL OR ends_at >= $1)
     ORDER BY display_name ASC`,
    [now.toISOString()]
  );

  return result.rows;
}

export async function seedLiveEventsIfEmpty(): Promise<void> {
  const count = await pool.query<{ count: string }>("SELECT COUNT(*)::text AS count FROM live_events");
  if (Number(count.rows[0]?.count ?? 0) > 0) {
    return;
  }

  await pool.query(
    `INSERT INTO live_events (
       event_id, scene_name, display_name, description,
       always_available, required_home_coins, starts_at, ends_at, is_enabled
     ) VALUES
       ('event_snow', 'Level02', 'Snow Festival', 'Limited snow biome rewards.', true, 0, NULL, NULL, true),
       ('event_coast', 'Level03', 'Fishing Coast', 'Catch rare fish for bonus coins.', false, 2000, NULL, NULL, true)
     ON CONFLICT (event_id) DO NOTHING`
  );
}
