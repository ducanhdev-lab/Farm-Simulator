# Farm Life Architecture

Implementation status (updated with TycoonCore foundation).

## Layers

- **Domain** — `PlayerProfile`, `EventWorldConfig`, `RecipeData`, `BuildingData`, `BiomeId`
- **Services** — `PlayerProfileService`, `WalletService`, `SaveCoordinator`, `EconomyService`, `WorldExpansionService`, `EventTravelService`, `LocalRemoteConfigService`, `DebugAnalyticsService`
- **Infrastructure** — `SaveSystem`, `ProfileMigration`, `ProfileMerger`, `GameEventBus`, `AnalyticsEventBridge`, `ApiClient`, `CloudSaveService`
- **Gameplay** — `IslandManager`, `Farm`, `ProcessingStation` (scene presenters)
- **Presentation** — `UIManager`, `HudController`, `PanelStack`, `InventoryWindow`

## Save flow

1. `IslandManager` collects world state into `GameData` (home) or event instance.
2. `SaveCoordinator` notifies collectors, then `PlayerProfileService.SaveNow()`.
3. Writes `player_profile.json` (+ legacy per-scene JSON for compatibility).
4. `CloudSaveService` PUTs to `/player/save` when online.
5. On **409 conflict** or **startup**: `ProfileMerger` merges by `saveVersion` / `lastSaveUtc`, saves local, retries upload.

## Scenes

| Scene | Role |
|-------|------|
| `Level01` | Home Island (persistent progression) |
| `Level02` | Snow event map |
| `Level03` | Coast event map |

Main menu always loads Home Island.

## Event bus

`GameEventBus` forwards to legacy `GameplayEvents` where needed. `QuestManager` subscribes only to `GameEventBus`. `AnalyticsEventBridge` forwards bus events to `IAnalyticsService`.

## Cloud save (Phase 1)

- Guest JWT persisted in `PlayerPrefs`
- Startup merge when cloud profile is newer
- Offline-first: local remains playable without server

## Biomes (Phase 2 prep)

`GameData.unlockedBiomeIds` tracks logical biomes (`BiomeId` enum). Default: `Starter` unlocked.

## Related docs

- [FARM_LIFE_DESIGN.md](FARM_LIFE_DESIGN.md) — target architecture & roadmap
- [server/README.md](../../server/README.md) — API & Docker
