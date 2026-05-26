using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IslandHarvest.Game
{
    public class PlayerProfileService : IPlayerProfileService
    {
        public static PlayerProfileService Instance { get; private set; }

        public PlayerProfile Profile { get; private set; }
        public GameData ActiveWorldData { get; private set; }
        public bool IsHomeWorld { get; private set; }

        public event Action<GameData> OnBeforeSave;
        public event Action<PlayerProfile> OnProfileSaved;

        private string activeSceneName;
        private bool isDirty;

        public static void EnsureInitialized()
        {
            if (Instance == null)
                Instance = new PlayerProfileService();
        }

        public void InitializeForScene(string sceneName, int startingCoins, List<bool> defaultUnlockedIslands)
        {
            Instance = this;
            activeSceneName = sceneName;
            IsHomeWorld = WorldSceneIds.IsHomeScene(sceneName);

            Profile = ProfileMigration.LoadOrMigrate();

            if (Profile == null)
            {
                Profile = new PlayerProfile(startingCoins, defaultUnlockedIslands);
                IsHomeWorld = true;
            }

            if (IsHomeWorld)
            {
                if (Profile.homeWorld == null)
                    Profile.homeWorld = new GameData(startingCoins, defaultUnlockedIslands);

                ActiveWorldData = Profile.homeWorld;
                ApplyCosmeticsPendingCoins();
            }
            else
            {
                string eventId = WorldSceneIds.GetEventIdForScene(sceneName);
                var instance = GetOrCreateEventInstance(eventId, sceneName);
                ActiveWorldData = BuildEventGameData(instance, startingCoins, defaultUnlockedIslands);
            }

            ProfileMigration.SyncCosmeticsToHomeWorld(Profile);
            isDirty = false;
        }

        private void ApplyCosmeticsPendingCoins()
        {
            if (Profile.cosmetics == null || Profile.homeWorld == null)
                return;

            if (Profile.cosmetics.pendingCoins <= 0)
                return;

            Profile.homeWorld.Coin += Profile.cosmetics.pendingCoins;
            Profile.cosmetics.pendingCoins = 0;
            isDirty = true;
        }

        private static GameData BuildEventGameData(EventInstanceData instance, int startingCoins, List<bool> defaultUnlocked)
        {
            var data = new GameData(instance.coins > 0 ? instance.coins : startingCoins, defaultUnlocked);
            if (instance.inventoryDatabase != null && instance.inventoryDatabase.Count > 0)
                data.InventoryDatabase = new List<InventoryData>(instance.inventoryDatabase);
            return data;
        }

        public EventInstanceData GetOrCreateEventInstance(string eventId, string sceneName)
        {
            var existing = Profile.eventInstances.FirstOrDefault(e => e.eventId == eventId);
            if (existing != null)
                return existing;

            existing = new EventInstanceData(eventId, sceneName);
            Profile.eventInstances.Add(existing);
            return existing;
        }

        public void RequestSave() => isDirty = true;

        public void SaveNow()
        {
            if (Profile == null || ActiveWorldData == null)
                return;

            OnBeforeSave?.Invoke(ActiveWorldData);

            if (IsHomeWorld)
            {
                ProfileMigration.SyncHomeWorldToCosmetics(Profile);
                Profile.homeWorld = ActiveWorldData;
            }
            else
            {
                PersistEventInstance(ActiveWorldData);
            }

            Profile.TouchSaveTimestamp();
            SaveSystem.SaveData(Profile, WorldSceneIds.ProfileFileName);

            // Legacy per-scene save for editor tools / backward compatibility
            if (!string.IsNullOrEmpty(activeSceneName))
                SaveSystem.SaveData(ActiveWorldData, activeSceneName);

            CosmeticsSave.Save(Profile.cosmetics);
            isDirty = false;
            OnProfileSaved?.Invoke(Profile);
        }

        private void PersistEventInstance(GameData worldData)
        {
            string eventId = WorldSceneIds.GetEventIdForScene(activeSceneName);
            var instance = GetOrCreateEventInstance(eventId, activeSceneName);
            instance.coins = worldData.Coin;
            instance.inventoryDatabase = worldData.InventoryDatabase ?? new List<InventoryData>();
            instance.lastPlayedUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
