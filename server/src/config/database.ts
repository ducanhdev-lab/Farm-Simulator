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
  `);
}
