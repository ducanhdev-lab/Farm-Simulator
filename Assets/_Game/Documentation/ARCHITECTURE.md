# Farm Life Architecture

## Layers

- **Domain** — Profile, recipes, buildings, biomes, snapshots, live event DTOs
- **Services** — Profile, wallet, economy, world, remote config, leaderboard, live events, snapshots
- **Infrastructure** — Save, cloud, API client, event bus
- **Gameplay** — Island, biomes, processing, travel
- **Presentation** — HUD, inventory, shop, **leaderboard**, island visit

## Online features

| Feature | Client | API |
|---------|--------|-----|
| Cloud save | `CloudSaveService` | `PUT /player/save` |
| Remote balance | `ApiRemoteConfigService` | `GET /config/balance` |
| Live events | `LiveEventsService` → `EventWorldRegistry` | `GET /events/active` |
| Leaderboard | `LeaderboardWindow` (Main Menu + Level01 HUD **RANK**) | `GET /leaderboard/coins` |
| Island visit | `IslandVisitWindow` | `GET /island/snapshot/:playerId` |
| Snapshot upload | `IslandSnapshotService` | `PUT /island/snapshot` (after save) |

## Editor setup

**Tools → Island Harvest → Setup Farm Life (All)** — processing, biomes, UI, inventory, **Main Menu + in-game leaderboard**

Or **Setup In-Game Leaderboard (UIManager)** / **Setup Main Menu Leaderboard** individually.

## Server

See [server/README.md](../../server/README.md). Restart `npm run dev` after pulling new routes.

## Editor (manual)

See [EDITOR_SETUP_GUIDE.md](EDITOR_SETUP_GUIDE.md) when wiring scenes/prefabs in Unity.

## Related

- [FARM_LIFE_DESIGN.md](FARM_LIFE_DESIGN.md)
