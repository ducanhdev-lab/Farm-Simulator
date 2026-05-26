# Farm Life — Sơ đồ thiết kế (Target: Project hoàn chỉnh)

Tài liệu mô tả **kiến trúc đích** khi Farm Life hoàn thành toàn bộ roadmap (Phase 1–5).  
Chú thích: **✅ đã có** | **🔶 một phần** | **⬜ kế hoạch**

---

## 1. Tổng quan hệ thống

```mermaid
flowchart TB
    subgraph client [Unity Client - Mobile]
        Game[Farm Life Game]
        LocalSave[(player_profile.json)]
        Game --> LocalSave
    end

    subgraph backend [Backend - Node.js TypeScript]
        API[Fastify REST API]
        WS[Socket.IO - Optional Phase 5]
        API --> PG[(PostgreSQL)]
        API --> Redis[(Redis Cache - Optional)]
        WS --> API
    end

    subgraph external [External Services]
        Store[App Store / Google Play IAP]
        Analytics[Analytics SDK]
        CDN[Addressables CDN]
    end

    Player((Player))
    Player --> Game
    Game <-->|HTTPS JWT| API
    Game --> Store
    Game --> Analytics
    Game --> CDN
    LocalSave -.->|sync khi online| API
```

---

## 2. Unity — Kiến trúc phân lớp (Clean / SOLID)

```mermaid
flowchart TB
    subgraph presentation [Presentation Layer]
        UI[UIManager]
        HUD[HudController]
        Panels[PanelStack / Windows]
        InvUI[InventoryWindow]
        ShopUI[GameShopWindow]
    end

    subgraph gameplay [Gameplay Layer - MonoBehaviour]
        IM[IslandManager - Scene Presenter]
        Farm[Farm / Soil]
        Silo[Silo / Farmer AI]
        Animals[Barn / Animal]
        Fish[FishingSpot]
        Build[Buildings / ProcessingStation]
        NPC[NPC Workers / Merchants]
    end

    subgraph services [Service Layer]
        GS[GameServices - Composition Root]
        Profile[IPlayerProfileService]
        Wallet[IWalletService]
        Save[ISaveCoordinator]
        Economy[IEconomyService]
        World[IWorldExpansionService]
        Nav[INavMeshWorldService]
        Travel[EventTravelService]
        Remote[IRemoteConfigService]
        AnalyticsSvc[IAnalyticsService]
    end

    subgraph domain [Domain Layer - Pure Data]
        PP[PlayerProfile]
        Items[ItemData / RecipeData / BuildingData]
        Events[EventWorldConfig / BiomeId]
        Quests[QuestData]
    end

    subgraph infra [Infrastructure]
        Bus[GameEventBus]
        SaveSys[SaveSystem + Migration]
        Pool[PoolManager]
        Net[ApiClient + CloudSaveService]
        Addr[Addressables Loader]
    end

    presentation --> services
    gameplay --> services
    services --> domain
    services --> infra
    gameplay --> Bus
    presentation --> Bus
```

| Lớp | Trách nhiệm | Trạng thái |
|-----|-------------|-----------|
| Domain | Models, SO config, không Unity lifecycle | 🔶 |
| Services | Business rules, wallet, economy, save | 🔶 |
| Gameplay | Scene objects, animation, triggers | ✅ |
| Presentation | HUD, windows, juice | 🔶 |
| Infrastructure | IO, network, pooling, events | 🔶 |

---

## 3. Core gameplay loop (Tycoon)

```mermaid
flowchart LR
    A[Gather Resources] --> B[Process Materials]
    B --> C[Sell Goods]
    C --> D[Earn Coins]
    D --> E[Expand Island]
    E --> F[Unlock Buildings]
    F --> G[Automation]
    G --> H[Optimize Production]
    H --> I[Scale Economy]
    I --> J[Events / Online]
    J --> A

    subgraph manual [Early Game]
        A
        B
        C
    end

    subgraph mid [Mid Game]
        E
        F
        G
    end

    subgraph late [Late Game]
        H
        I
        J
    end
```

---

## 4. Cấu trúc thế giới & Scene

```mermaid
flowchart TB
    Menu[_MainMenu]
    Home[Home Island - Level01]
    EventSnow[Event: Snow - Level02]
    EventCoast[Event: Coast - Level03]
    FutureBiomes[Future Biome Scenes - Addressables]

    Menu -->|Start| Home
    Menu -->|Event Portal| EventSnow
    Menu -->|Event Portal| EventCoast
    Home -->|EventTravelService| EventSnow
    Home -->|EventTravelService| EventCoast
    EventSnow -->|Return Home| Home
    EventCoast -->|Return Home| Home
    Home -.->|unlock progress| FutureBiomes

    subgraph profile [PlayerProfile]
        HW[homeWorld - persistent]
        EI[eventInstances - per event]
        COS[cosmetics + IAP]
    end

    Home --> HW
    EventSnow --> EI
    EventCoast --> EI
```

### Biome progression (logical trên Home — ⬜ Phase 2+)

```mermaid
flowchart LR
    Starter[Starter Farm] --> Fishing[Fishing Coast zone]
    Fishing --> Mountain[Mountain / Mining]
    Mountain --> Snow[Snow Island tiles]
    Snow --> Industrial[Industrial / Factory]
    Industrial --> Mystic[Mystic / Rare materials]
```

---

## 5. Mô hình dữ liệu — PlayerProfile

```mermaid
erDiagram
    PlayerProfile ||--|| HomeWorldData : contains
    PlayerProfile ||--|| CosmeticsData : contains
    PlayerProfile ||--o{ EventInstanceData : has

    PlayerProfile {
        string profileId
        int saveVersion
        string lastSaveUtc
    }

    HomeWorldData {
        int Coin
        list unlockedIslands
        list inventories
        list silos
        int array animalLevels
        list questProgress
        map biomeFlags
    }

    EventInstanceData {
        string eventId
        string sceneName
        int coins
        list inventories
    }

    CosmeticsData {
        list ownedSkinIds
        string equippedSkinId
        list completedIAPProductIds
    }
```

**Nguồn sự thật:** `player_profile.json` (local) ↔ cloud JSONB (server).

---

## 6. Economy & Production pipeline

```mermaid
flowchart TB
    subgraph sources [Sources]
        Crop[Farm Harvest]
        Tree[Fruit Tree]
        Animal[Barn Products]
        FishMinigame[Fishing]
        Mine[Mining - future]
    end

    subgraph storage [Storage]
        PlayerInv[Player Inventory]
        SiloInv[Silo / Warehouse]
    end

    subgraph process [Processing]
        Recipe[RecipeData SO]
        Station[ProcessingStation / Factory]
    end

    subgraph market [Market]
        Stall[Stall - dynamic price]
        Merchant[Rotating Merchant]
        MarketAPI[Remote Market Config]
    end

    subgraph wallet_layer [Wallet]
        W[IWalletService]
    end

    sources --> PlayerInv
    sources --> SiloInv
    PlayerInv --> Station
    SiloInv --> Station
    Recipe --> Station
    Station --> PlayerInv
    PlayerInv --> Stall
    Stall --> EconomySvc[EconomyService]
    EconomySvc --> W
    MarketAPI -.->|balance| Stall
    W --> HomeWorldData
```

---

## 7. Automation & NPC (Target)

```mermaid
stateDiagram-v2
    [*] --> Idle
    Idle --> Pathfinding: has WorkOrder
    Pathfinding --> Working: reached target
    Working --> Idle: job complete
    Idle --> Rest: schedule off-hours

    note right of Working
        Farmer / WorkerAgent
        Auto harvest / mine / fish machine
        Conveyor input-output
    end note
```

| Thành phần | Vai trò | Trạng thái |
|------------|---------|-----------|
| `Farmer` (NavMesh) | Auto farm loop per silo | ✅ |
| `WorkerAgent` + `WorkOrder` | Generic worker pool | ⬜ |
| Conveyor / machines | Industrial biome | ⬜ |
| NPC schedule + state machine | Merchants, customers | ⬜ |

---

## 8. Event bus & hệ thống phụ thuộc

```mermaid
flowchart LR
    subgraph publishers [Publishers]
        Soil[Soil Harvest]
        StallNode[Stall Sell]
        Island[Island Unlock]
        SiloUp[Silo Upgrade]
        QuestSrc[Quest Actions]
    end

    Bus[GameEventBus]

    subgraph subscribers [Subscribers]
        QM[QuestManager]
        HUD2[CoinDisplay / UI]
        AN[IAnalyticsService]
        ACH[AchievementSystem]
    end

    Soil --> Bus
    StallNode --> Bus
    Island --> Bus
    SiloUp --> Bus
    QuestSrc --> Bus
    Bus --> QM
    Bus --> HUD2
    Bus --> AN
    Bus --> ACH
```

---

## 9. Luồng Save & Cloud (Offline-first)

```mermaid
sequenceDiagram
    participant G as Gameplay
    participant IM as IslandManager
    participant SC as SaveCoordinator
    participant PS as PlayerProfileService
    participant L as Local JSON
    participant CS as CloudSaveService
    participant API as Backend API

    G->>IM: gameplay changes
    IM->>SC: RequestSave / OnDisable
    SC->>IM: OnCollectSaveState
    SC->>PS: SaveNow
    PS->>L: player_profile.json
    PS-->>CS: OnProfileSaved
    CS->>API: PUT /player/save
    alt version conflict
        API-->>CS: 409
        CS-->>G: merge / prompt user
    else success
        API-->>CS: 200 ok
    end
```

---

## 10. Backend — Kiến trúc đích (Phase 1–5)

```mermaid
flowchart TB
    subgraph routes [Routes]
        Auth["/auth/guest"]
        Profile["/player/profile"]
        Save["/player/save"]
        LB["/leaderboard/*"]
        Events["/events/active"]
        Snap["/island/snapshot"]
        Trade["/marketplace - optional"]
    end

    subgraph layers [Server Layers]
        MW[Middlewares - JWT rate limit]
        CTRL[Controllers]
        SVC[Services]
        REPO[Repositories]
    end

  Auth --> MW
    Profile --> MW
    Save --> MW
    LB --> MW
    Events --> MW
    Snap --> MW
    MW --> CTRL --> SVC --> REPO --> PG[(PostgreSQL)]

    SVC --> Redis[(Redis)]
```

| Phase | API / Feature | Trạng thái |
|-------|----------------|-----------|
| 1 | Guest auth, profile, cloud save | 🔶 |
| 2 | Leaderboard + anti-cheat validation | ⬜ |
| 3 | Live events, remote balancing | ⬜ |
| 4 | Async island visit / snapshot | ⬜ |
| 5 | Realtime trade, guild, chat | ⬜ |

---

## 11. UI / UX kiến trúc (Target)

```mermaid
flowchart TB
    subgraph hud [Always Visible]
        Coin[CoinDisplay]
        Items[Item HUD strip]
        Joy[Joystick]
    end

    subgraph stack [PanelStack]
        Settings[SettingsWindow]
        SiloUI[SiloUpgrade / SiloInfo]
        BarnUI[AnimalUpgrade]
        MerchantUI[MerchantInfo]
        InvWin[InventoryWindow - grid filter]
        QuestUI[QuestHUD]
    end

    subgraph meta [Meta Screens]
        MainMenu[MainMenu - Home / Events / Shop]
        Shop[GameShopWindow - Skins IAP]
        LeaderboardUI[Leaderboard - future]
    end

    UIRoot[UIManager] --> hud
    UIRoot --> stack
    MainMenu --> meta
```

**Style hướng:** bright, cozy, mobile-first (Dreamdale / Family Farm Adventure).

---

## 12. Monetization & Analytics (Soft)

```mermaid
flowchart LR
    IAP[IAPService] --> Cosmetics[Cosmetics in Profile]
    Ads[Rewarded Ads] --> Wallet
    Pass[Premium Pass] --> RemoteConfig
    Skins[Cosmetic Skins] --> PlayerVisual

    Gameplay --> Analytics[IAnalyticsService]
    Analytics --> BackendMetrics[Optional server ingest]
```

Không pay-to-win nặng: cosmetics, convenience, battle pass style.

---

## 13. Performance & Content delivery

```mermaid
flowchart LR
    subgraph runtime [Runtime Optimization]
        Pool[Object Pooling]
        Chunk[Chunk / Island culling]
        Instancing[GPU Instancing]
        NavBatch[Deferred NavMesh rebuild]
    end

    subgraph content [Content Pipeline]
        SO[ScriptableObject configs]
        Addr[Addressables]
        Stream[Scene streaming]
    end

    SO --> Gameplay
    Addr --> EventScenes
    Addr --> BiomeAssets
```

---

## 14. Map triển khai: Hiện tại → Hoàn chỉnh

```mermaid
gantt
    title Farm Life Roadmap
    dateFormat YYYY-MM
    section Foundation
        PlayerProfile Services EventBus     :done, 2025-01, 2025-02
        Backend scaffold CloudSave          :active, 2025-02, 2025-03
    section TycoonCore
        Economy recipes buildings biomes    :2025-03, 2025-05
        UI polish inventory automation      :2025-04, 2025-06
    section Polish
        VFX juice analytics docs            :2025-06, 2025-07
    section BackendLive
        Leaderboard live events visits      :2025-07, 2025-09
    section Optional
        Realtime multiplayer                :2025-09, 2025-11
```

---

## 15. Nguyên tắc thiết kế (không đổi)

1. **Offline-first** — local profile là source of truth; cloud merge theo `saveVersion`.
2. **IslandManager = presenter** — không gom economy + save + mesh forever.
3. **Data-driven** — balance qua SO + remote config, không hardcode trong client.
4. **Event maps tách progression** — Home persistent, event instance riêng.
5. **Mobile performance first** — pool, chunk, async load.

---

## Tài liệu liên quan

- [ARCHITECTURE.md](ARCHITECTURE.md) — trạng thái implementation hiện tại
- [server/README.md](../../server/README.md) — API & Docker
- Master plan — `.cursor/plans/farm_life_refactor_*.plan.md`
