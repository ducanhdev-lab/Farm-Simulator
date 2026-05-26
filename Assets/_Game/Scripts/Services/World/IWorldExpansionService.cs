namespace IslandHarvest.Game
{
    public interface IWorldExpansionService
    {
        int CalculateIslandUnlockPrice(int islandIndex);
        bool IsBiomeUnlocked(GameData world, BiomeId biome);
        void UnlockBiome(GameData world, BiomeId biome);
    }
}
