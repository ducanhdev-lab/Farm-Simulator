using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class ProfileMigration
    {
        public static PlayerProfile LoadOrMigrate()
        {
            var profile = SaveSystem.LoadData<PlayerProfile>(WorldSceneIds.ProfileFileName);
            if (profile != null && profile.homeWorld != null)
            {
                EnsureProfileDefaults(profile);
                return profile;
            }

            profile = MigrateFromLegacySaves();
            if (profile != null)
            {
                SaveSystem.SaveData(profile, WorldSceneIds.ProfileFileName);
                Debug.Log("Migrated legacy saves to player_profile.json");
            }

            return profile;
        }

        private static PlayerProfile MigrateFromLegacySaves()
        {
            string homeSave = SaveSystem.SaveFileExists(WorldSceneIds.HomeIsland)
                ? WorldSceneIds.HomeIsland
                : SaveSystem.GetLatestSaveFileName();

            GameData homeData = null;
            if (!string.IsNullOrEmpty(homeSave))
                homeData = SaveSystem.LoadData<GameData>(homeSave);

            var cosmetics = CosmeticsSave.Load();
            if (homeData == null)
                return null;

            CosmeticsSave.MergeFromLevelSave(homeData);
            CosmeticsSave.ApplyPendingCoinsToGameData(homeData);

            var profile = new PlayerProfile
            {
                homeWorld = homeData,
                cosmetics = cosmetics
            };

            MigrateEventSceneSaves(profile);
            profile.TouchSaveTimestamp();
            EnsureProfileDefaults(profile);
            return profile;
        }

        private static void MigrateEventSceneSaves(PlayerProfile profile)
        {
            foreach (string scene in new[] { WorldSceneIds.SnowEvent, WorldSceneIds.CoastEvent })
            {
                if (!SaveSystem.SaveFileExists(scene))
                    continue;

                var legacy = SaveSystem.LoadData<GameData>(scene);
                if (legacy == null)
                    continue;

                string eventId = WorldSceneIds.GetEventIdForScene(scene);
                profile.eventInstances.Add(new EventInstanceData(eventId, scene)
                {
                    coins = legacy.Coin,
                    inventoryDatabase = legacy.InventoryDatabase ?? new List<InventoryData>()
                });
            }
        }

        private static void EnsureProfileDefaults(PlayerProfile profile)
        {
            if (string.IsNullOrEmpty(profile.profileId))
                profile.profileId = System.Guid.NewGuid().ToString("N");

            if (profile.cosmetics == null)
                profile.cosmetics = new CosmeticsData();

            if (profile.eventInstances == null)
                profile.eventInstances = new List<EventInstanceData>();

            if (profile.homeWorld == null)
                return;

            if (profile.homeWorld.ownedSkinIds == null)
                profile.homeWorld.ownedSkinIds = new List<string> { PlayerSkinManager.DefaultSkinId };

            if (string.IsNullOrEmpty(profile.homeWorld.equippedSkinId))
                profile.homeWorld.equippedSkinId = PlayerSkinManager.DefaultSkinId;

            if (profile.homeWorld.completedIAPProductIds == null)
                profile.homeWorld.completedIAPProductIds = new List<string>();

            SyncCosmeticsToHomeWorld(profile);
        }

        public static void SyncCosmeticsToHomeWorld(PlayerProfile profile)
        {
            if (profile?.homeWorld == null || profile.cosmetics == null)
                return;

            profile.homeWorld.ownedSkinIds = new List<string>(profile.cosmetics.ownedSkinIds);
            profile.homeWorld.equippedSkinId = profile.cosmetics.equippedSkinId;
            profile.homeWorld.completedIAPProductIds = new List<string>(profile.cosmetics.completedIAPProductIds);
        }

        public static void SyncHomeWorldToCosmetics(PlayerProfile profile)
        {
            if (profile?.homeWorld == null || profile.cosmetics == null)
                return;

            profile.cosmetics.ownedSkinIds = new List<string>(profile.homeWorld.ownedSkinIds);
            profile.cosmetics.equippedSkinId = profile.homeWorld.equippedSkinId;
            profile.cosmetics.completedIAPProductIds = new List<string>(profile.homeWorld.completedIAPProductIds);
        }
    }
}
