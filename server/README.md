# Farm Life API

Node.js + TypeScript + Fastify + PostgreSQL backend for cloud save.

## Quick start

```bash
cd server
docker compose up -d postgres
npm install
npm run dev
```

API: `http://localhost:3000`

## Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health` | No | Health check |
| POST | `/auth/guest` | No | Returns `{ token, playerId }` |
| GET | `/player/profile` | Bearer JWT | Player profile JSON |
| PUT | `/player/save` | Bearer JWT | Body: `{ saveVersion, profileJson }` |

## Unity

Set `CloudSaveService.apiBaseUrl` to your API URL. Guest auth runs on start; profile syncs after each save.
