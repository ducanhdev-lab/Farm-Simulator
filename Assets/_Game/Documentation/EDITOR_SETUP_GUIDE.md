# Farm Life — Hướng dẫn Unity Editor (làm khi sẵn sàng)

Các tính năng **runtime / server** đã code sẵn; phần dưới chỉ cần chạy **một lần** (hoặc sau khi pull scene/prefab mới).

## 1. Setup tổng (khuyến nghị)

**Menu:** `Tools → Island Harvest → Setup Farm Life (All)`

Tự động:

- Recipe + building + `ProcessingStation` trên Level01  
- `BiomeProgression` + `BiomeZone` (Starter, Fishing Coast)  
- `HudController`, `PanelStack`, `InventoryWindow` trên prefab UIManager  
- Nút **RANK** + `LeaderboardWindow` in-game  
- Leaderboard + Visit trên **Main Menu**

## 2. Từng bước riêng (nếu cần)

| Menu | Khi nào dùng |
|------|----------------|
| **Setup Processing (Recipe + Home Station)** | Thiếu recipe/building/prefab chế biến |
| **Setup Biome Zones (Level01)** | Vùng biome chưa khóa/mở đúng |
| **Setup Modular UI (UIManager Prefab)** | Thiếu HUD modular / inventory |
| **Setup In-Game Leaderboard (UIManager)** | Level01 không có nút RANK |
| **Polish Inventory Window (UIManager)** | Inventory thiếu nút đóng / summary |
| **Setup Main Menu Leaderboard** | Main Menu thiếu nút RANK |

## 3. Không cần Editor

| Tính năng | Ghi chú |
|-----------|---------|
| **Save conflict (409)** | Popup runtime khi cloud save xung đột |
| **Demo leaderboard** | Server seed 15 người chơi giả |
| **displayName trên rank** | Lấy từ `profile_json` / demo seed |

## 4. Server local

```bash
cd server
docker compose up -d postgres
npm run dev
```

- Seed rank: `POST http://localhost:3000/dev/seed-leaderboard` (body `{}`)  
- Xem/sửa event: `GET /dev/events`, `PUT /dev/events/event_coast`

## 5. Test nhanh sau setup

1. Play **Level01** → **RANK** → thấy FarmerAce, CropQueen…  
2. Bấm một hàng → **Island Visit** (text snapshot)  
3. Gây conflict 409 (2 máy save) → chọn Merge / Device / Cloud  

## Liên quan

- [FARM_LIFE_DESIGN.md](FARM_LIFE_DESIGN.md)  
- [ARCHITECTURE.md](ARCHITECTURE.md)  
- [server/README.md](../../../server/README.md)
