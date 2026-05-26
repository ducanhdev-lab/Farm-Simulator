# IAP Store Setup — Island Harvest

## Product catalog (must match store consoles)

| ScriptableObject | Store Product ID | Type | Reward |
|------------------|------------------|------|--------|
| IAP_Coins_Small | `iap_coins_small` | Consumable | 2,500 coins |
| IAP_Coins_Large | `iap_coins_large` | Consumable | 10,000 coins |
| IAP_Skin_Gold | `iap_skin_gold` | Non-consumable | Skin `skin_gold` |

Assets live under `Assets/_Game/Resources/IAP/`.

## Google Play Console

1. Create in-app products with IDs exactly as above.
2. Set pricing and activate products.
3. Add license testers for sandbox purchases.
4. Upload a build with `com.unity.purchasing` enabled.

## App Store Connect

1. Create consumable products for coin packs.
2. Create non-consumable for `iap_skin_gold`.
3. Add Sandbox Apple ID testers.
4. Enable In-App Purchase capability on the app ID.

## Unity Editor

1. **Tools → Island Harvest → Create Skin Catalog Asset** (một lần) — chỉnh skin trong Inspector, bấm **Đồng bộ vào Shop**
2. **Tools → Island Harvest → Create Skin Shop Assets** (prefab shop UI, nếu chưa có)
3. **Tools → Island Harvest → Setup Main Menu Shop Window**
3. Window **Services → In-App Purchasing** (optional UGS link).

Shop, skin purchases, and IAP run **only** on `_MainMenu` (no in-level store).

## Test checklist

- [ ] Main menu: Shop → tab **Skins** — buy Blue/Purple with coins; coin label updates.
- [ ] Main menu: tab **IAP** — coin pack grants coins (or `pendingCoins` if no save yet).
- [ ] Main menu: buy premium Gold skin; appears owned; Equip works.
- [ ] Main menu: equip Purple → **Start** → in level, player mesh uses Purple material.
- [ ] After IAP coin pack from main menu, coins persist; first level load merges `pendingCoins` into `GameData.Coin`.
- [ ] Fresh install, no save: coin IAP increases `pendingCoins`; first level load merges into `GameData.Coin`.

## Notes

- Client-side entitlements only (`completedIAPProductIds` in `Cosmetics.json` / `GameData`).
- Editor without store: Unity Fake Store or simulated grant when Purchasing package is absent.
