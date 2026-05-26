using System.Linq;
using UnityEngine;

namespace IslandHarvest.Game
{
    public class WorldExpansionService : IWorldExpansionService
    {
        private readonly int baseUnlockPrice;
        private readonly float unlockGrowthFactor;

        public WorldExpansionService(int baseUnlockPrice = 100, float unlockGrowthFactor = 1.1f)
        {
            this.baseUnlockPrice = baseUnlockPrice;
            this.unlockGrowthFactor = unlockGrowthFactor;
        }

        public int CalculateIslandUnlockPrice(int islandIndex) =>
            Mathf.RoundToInt(Mathf.Round(baseUnlockPrice * Mathf.Pow(unlockGrowthFactor, islandIndex)) / 5f) * 5;

        public bool IsBiomeUnlocked(GameData world, BiomeId biome)
        {
            if (world?.unlockedBiomeIds == null)
                return biome == BiomeId.Starter;

            string id = biome.ToString();
            return world.unlockedBiomeIds.Contains(id);
        }

        public void UnlockBiome(GameData world, BiomeId biome)
        {
            if (world == null)
                return;

            world.unlockedBiomeIds ??= new System.Collections.Generic.List<string>();
            string id = biome.ToString();
            if (!world.unlockedBiomeIds.Contains(id))
                world.unlockedBiomeIds.Add(id);
        }
    }
}
