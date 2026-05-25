# Farming Island

**3D hyper-casual farming simulator** — mobile-first, island expansion, idle automation.

---

## 1. Đây là game gì?

Người chơi khám phá các đảo, trồng trọt, chăn nuôi, câu cá, thu hoạch và bán hàng để kiếm coin, mở khóa đảo mới, nâng cấp silo / farmer / barn, rồi qua 3 level.

**Core loop:** farm → thu hoạch → bán → mở đảo → automate → next level.

---

## 2. Bạn làm gì?

- **Gameplay systems:** crop farming (plant / water / harvest), silo + auto farmer (NavMesh), island unlock, inventory & economy, stall / merchant, fruit tree, barn (poultry / dairy / fiber), fishing minigame, level progression.
- **Mobile production:** touch joystick, haptics, settings (SFX / BGM / vibration / shadows / render scale), portrait-friendly UI, 60 FPS target.
- **Architecture & data:** ScriptableObject-driven items / crops / animals; per-level binary save; manager-based scene flow (island, UI, audio, graphics, pool).
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
| **Save** | Per-scene `.dat` binary save + Editor tool (`Tools → Farming Island → Save File Manager`) |

**Stack:** Unity 6000.3 · URP 17 · Cinemachine · AI Navigation 2 · DOTween · SimpleInput

---

## 4. Có playable / video không?
