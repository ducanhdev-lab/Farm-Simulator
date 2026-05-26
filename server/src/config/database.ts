import pg from "pg";
import { env } from "./env.js";

export const pool = new pg.Pool({ connectionString: env.databaseUrl });

export async function initDatabase(): Promise<void> {
  await pool.query(`
    CREATE TABLE IF NOT EXISTS players (
      id UUID PRIMARY KEY,
      save_version INT NOT NULL DEFAULT 1,
      profile_json JSONB NOT NULL DEFAULT '{}',
      created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
      updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
    );

    CREATE TABLE IF NOT EXISTS leaderboard_scores (
      player_id UUID PRIMARY KEY REFERENCES players(id) ON DELETE CASCADE,
      display_coins INT NOT NULL DEFAULT 0,
      save_version INT NOT NULL DEFAULT 1,
      updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
    );

    CREATE INDEX IF NOT EXISTS idx_leaderboard_coins
      ON leaderboard_scores (display_coins DESC);

    CREATE TABLE IF NOT EXISTS island_snapshots (
      player_id UUID PRIMARY KEY REFERENCES players(id) ON DELETE CASCADE,
      snapshot_json JSONB NOT NULL DEFAULT '{}',
      updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
    );

    CREATE TABLE IF NOT EXISTS live_events (
      event_id TEXT PRIMARY KEY,
      scene_name TEXT NOT NULL,
      display_name TEXT NOT NULL,
      description TEXT NOT NULL DEFAULT '',
      always_available BOOLEAN NOT NULL DEFAULT false,
      required_home_coins INT NOT NULL DEFAULT 0,
      starts_at TIMESTAMPTZ,
      ends_at TIMESTAMPTZ,
      is_enabled BOOLEAN NOT NULL DEFAULT true
    );
  `);
}
