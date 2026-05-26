import { pool } from "../config/database.js";
/** Fixed UUIDs — demo rows, safe to upsert on every dev boot. */
const DEMO_ENTRIES = [
    { playerId: "11111111-1111-1111-1111-111111111101", coins: 248_500, label: "FarmerAce" },
    { playerId: "11111111-1111-1111-1111-111111111102", coins: 192_300, label: "CropQueen" },
    { playerId: "11111111-1111-1111-1111-111111111103", coins: 156_750, label: "SiloKing" },
    { playerId: "11111111-1111-1111-1111-111111111104", coins: 134_200, label: "HarvestPro" },
    { playerId: "11111111-1111-1111-1111-111111111105", coins: 98_600, label: "BarnBoss" },
    { playerId: "11111111-1111-1111-1111-111111111106", coins: 76_400, label: "DockDiver" },
    { playerId: "11111111-1111-1111-1111-111111111107", coins: 54_800, label: "SeedSprout" },
    { playerId: "11111111-1111-1111-1111-111111111108", coins: 41_250, label: "MilkMaven" },
    { playerId: "11111111-1111-1111-1111-111111111109", coins: 28_900, label: "TractorTim" },
    { playerId: "11111111-1111-1111-1111-111111111110", coins: 19_500, label: "HayHay" },
    { playerId: "11111111-1111-1111-1111-111111111111", coins: 12_400, label: "NewbieNina" },
    { playerId: "11111111-1111-1111-1111-111111111112", coins: 8_750, label: "CoinCatcher" },
    { playerId: "11111111-1111-1111-1111-111111111113", coins: 5_200, label: "IslandIan" },
    { playerId: "11111111-1111-1111-1111-111111111114", coins: 2_800, label: "StarterSam" },
    { playerId: "11111111-1111-1111-1111-111111111115", coins: 950, label: "FreshFarmer" },
];
export function isDemoLeaderboardSeedingEnabled() {
    if (process.env.SEED_DEMO_LEADERBOARD === "false") {
        return false;
    }
    if (process.env.NODE_ENV === "production" && process.env.SEED_DEMO_LEADERBOARD !== "true") {
        return false;
    }
    return true;
}
export async function seedDemoLeaderboard() {
    if (!isDemoLeaderboardSeedingEnabled()) {
        return 0;
    }
    let seeded = 0;
    for (const entry of DEMO_ENTRIES) {
        const profile = {
            profileId: entry.playerId,
            saveVersion: 1,
            displayName: entry.label,
            homeWorld: { Coin: entry.coins },
        };
        await pool.query(`INSERT INTO players (id, save_version, profile_json, updated_at)
       VALUES ($1, 1, $2::jsonb, NOW())
       ON CONFLICT (id) DO UPDATE SET
         profile_json = EXCLUDED.profile_json,
         updated_at = NOW()`, [entry.playerId, JSON.stringify(profile)]);
        await pool.query(`INSERT INTO leaderboard_scores (player_id, display_coins, save_version, updated_at)
       VALUES ($1, $2, 1, NOW())
       ON CONFLICT (player_id) DO UPDATE SET
         display_coins = EXCLUDED.display_coins,
         save_version = EXCLUDED.save_version,
         updated_at = NOW()`, [entry.playerId, entry.coins]);
        const snapshot = {
            coins: entry.coins,
            unlockedIslandCount: Math.min(5, Math.floor(entry.coins / 25_000) + 1),
            equippedSkinId: "default",
            capturedUtc: new Date().toISOString(),
            displayName: entry.label,
        };
        await pool.query(`INSERT INTO island_snapshots (player_id, snapshot_json, updated_at)
       VALUES ($1, $2::jsonb, NOW())
       ON CONFLICT (player_id) DO UPDATE SET
         snapshot_json = EXCLUDED.snapshot_json,
         updated_at = NOW()`, [entry.playerId, JSON.stringify(snapshot)]);
        seeded += 1;
    }
    return seeded;
}
