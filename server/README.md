# Farm Life API

Node.js + TypeScript + Fastify + PostgreSQL backend for cloud save.

## Quick start

**Requires:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL) and Node.js 20+.

```bash
cd server
docker compose up -d postgres
npm install
npm run dev
```

If `docker` is not installed, install Docker Desktop and ensure it is running, then retry `docker compose up -d postgres`.

API: `http://localhost:3000`

**Note:** Opening API URLs in the browser uses **GET**. Endpoints like `/auth/guest` require **POST** (Unity and curl/Postman use POST correctly).

Quick test (PowerShell):

```powershell
Invoke-RestMethod -Uri "http://localhost:3000/auth/guest" -Method POST -ContentType "application/json" -Body "{}"
```

## Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health` | No | Health check |
| POST | `/auth/guest` | No | Returns `{ token, playerId }` |
| GET | `/config/balance` | No | Stall multipliers + island base price |
| GET | `/events/active` | No | Live event maps (schedule + gates) |
| GET | `/leaderboard/coins?limit=20` | No | Top players by home coins |
| PUT | `/island/snapshot` | Bearer JWT | Upload visit snapshot |
| GET | `/island/snapshot/:playerId` | Bearer JWT | View another player's snapshot |
| GET | `/player/profile` | Bearer JWT | Player profile JSON |
| PUT | `/player/save` | Bearer JWT | Body: `{ saveVersion, profileJson }` (coin delta + rate limit) |

## Unity

Set `CloudSaveService.apiBaseUrl` to your API URL. Guest auth runs on start; profile syncs after each save.

**Live events:** rows in PostgreSQL `live_events` (seeded on first boot). Update `starts_at` / `ends_at` via SQL or future admin API.

**Anti-cheat:** rejects saves when coin gain exceeds per-save or per-minute limits vs. last cloud profile.

### Demo leaderboard (local testing)

On startup (non-production), the server upserts **15 fake players** (coins 950 → 248,500) with island snapshots for visit testing.

Re-seed without restart:

```powershell
Invoke-RestMethod -Uri "http://localhost:3000/dev/seed-leaderboard" -Method POST
```

Then open **RANK** in Unity (Main Menu or Level01). Demo player IDs start with `11111111-…`.

Disable in production: `SEED_DEMO_LEADERBOARD=false`
