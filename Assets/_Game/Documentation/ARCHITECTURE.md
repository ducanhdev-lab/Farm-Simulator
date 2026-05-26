# Farm Life Architecture

## Layers

- **Domain** — `PlayerProfile`, `EventWorldConfig`, `RecipeData` (serializable models + ScriptableObjects)
- **Services** — `PlayerProfileService`, `WalletService`, `SaveCoordinator`, `EconomyService`, `EventTravelService`
- **Infrastructure** — `SaveSystem`, `ProfileMigration`, `GameEventBus`, `ApiClient`, `CloudSaveService`
- **Gameplay** — `IslandManager`, `Farm`, `ProcessingStation` (scene presenters)
- **Presentation** — `UIManager`, `HudController`, `PanelStack`, `InventoryWindow`

## Save flow

1. `IslandManager` collects world state into `GameData` (home) or event instance.
2. `SaveCoordinator` notifies collectors, then `PlayerProfileService.SaveNow()`.
3. Writes `player_profile.json` (+ legacy per-scene JSON for compatibility).
4. `CloudSaveService` PUTs to `/player/save` when online.

## Scenes

| Scene | Role |
|-------|------|
| `Level01` | Home Island (persistent progression) |
| `Level02` | Snow event map |
| `Level03` | Coast event map |

Main menu always loads Home Island.

## Event bus

`GameEventBus` forwards to legacy `GameplayEvents` where needed for quests.
