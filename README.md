# Farming Island

**Persistent island tycoon** — mobile-first, home island progression, seasonal event maps, cloud-save ready backend.

---

## 1. Đây là game gì?

Người chơi xây dựng **Home Island** (Level01), trồng trọt, chăn nuôi, câu cá, bán hàng, mở khóa đảo, automate — progression lưu trong `player_profile.json`. Level02/03 là **event maps** (tuyết, coast).

**Core loop:** gather → process → sell → expand → automate → scale economy.

---

## 2. Bạn làm gì?

- **Gameplay systems:** crop farming (plant / water / harvest), silo + auto farmer (NavMesh), island unlock, inventory & economy, stall / merchant, fruit tree, barn (poultry / dairy / fiber), fishing minigame, level progression.
- **Mobile production:** touch joystick, haptics, settings (SFX / BGM / vibration / shadows / render scale), portrait-friendly UI, 60 FPS target.
- **Architecture & data:** `PlayerProfile` + services (`Wallet`, `SaveCoordinator`, `Economy`); `GameEventBus`; ScriptableObject items/recipes/events; Node/Fastify cloud save API in `server/`.
- **Performance pass:** object pooling, island spatial grid, batched NavMesh rebuild, URP render-scale tuning.

---

## 3. Có gì technical đáng chú ý?

| Area | Detail |
|------|--------|
| **Pooling** | Crops, fruit drops, stall items, coin VFX — `PoolManager` + DOTween capacity preset |
| **Island expansion** | `Dictionary<Vector3Int, Island>` spatial grid + incremental mesh refresh — tránh CPU spike trên map lớn |
| **Automation** | NavMesh farmers chạy farm loop; delayed NavMesh update khi spawn prop hàng loạt |
| **Mobile graphics** | URP render scale 0.5–1.0, shadow toggle, Metal/iOS water fix, sprite atlas |
| **Data-driven** | 34+ items, crop / animal configs tách khỏi logic — dễ balance & mở rộng content |
| **Save** | `player_profile.json` (v2) + cloud sync via `CloudSaveService` + legacy per-scene migration |

**Stack:** Unity 6000.3 · URP 17 · Cinemachine · AI Navigation 2 · DOTween · SimpleInput

---

## 4. Có playable / video không?
