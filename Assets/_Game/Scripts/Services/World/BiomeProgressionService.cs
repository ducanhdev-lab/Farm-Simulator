namespace IslandHarvest.Game
{
    public class BiomeProgressionService
    {
        private readonly IWorldExpansionService world;
        private readonly BiomeProgressionConfig config;

        public BiomeProgressionService(IWorldExpansionService world, BiomeProgressionConfig config)
        {
            this.world = world;
            this.config = config;
        }

        public void EvaluateAutoUnlocks(GameData data)
        {
            if (data == null || config == null)
                return;

            world.UnlockBiome(data, BiomeId.Starter);

            foreach (var gate in config.Gates)
            {
                if (gate == null || gate.biome == BiomeId.Starter)
                    continue;

                if (world.IsBiomeUnlocked(data, gate.biome))
                    continue;

                if (!world.IsBiomeUnlocked(data, gate.requiresBiome))
                    continue;

                if (data.Coin < gate.requiredCoins)
                    continue;

                world.UnlockBiome(data, gate.biome);
            }
        }
    }
}
