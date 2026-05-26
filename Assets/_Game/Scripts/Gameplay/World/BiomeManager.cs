using UnityEngine;

namespace IslandHarvest.Game
{
    public class BiomeManager : MonoBehaviour
    {
        [SerializeField] private BiomeProgressionConfig progressionConfig;
        [SerializeField] private BiomeZone[] zones;

        private BiomeProgressionService progression;
        private IWorldExpansionService world;
        private GameData worldData;

        private void Start()
        {
            world = GameServices.Instance?.World;
            worldData = GameServices.Instance?.Profile?.ActiveWorldData;

            if (progressionConfig != null && world != null)
                progression = new BiomeProgressionService(world, progressionConfig);

            if (zones == null || zones.Length == 0)
                zones = FindObjectsByType<BiomeZone>(FindObjectsSortMode.None);

            if (IslandManager.Instance != null)
                IslandManager.Instance.OnCoinChanged += HandleCoinChanged;

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (IslandManager.Instance != null)
                IslandManager.Instance.OnCoinChanged -= HandleCoinChanged;
        }

        private void HandleCoinChanged(int _) => RefreshAll();

        public void RefreshAll()
        {
            worldData = GameServices.Instance?.Profile?.ActiveWorldData;
            if (worldData == null)
                return;

            progression?.EvaluateAutoUnlocks(worldData);

            foreach (var zone in zones)
            {
                if (zone != null)
                    zone.Refresh(world, worldData);
            }
        }
    }
}
