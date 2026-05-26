using System;
using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    [CreateAssetMenu(fileName = "BiomeProgression", menuName = "Island Harvest/Biome Progression")]
    public class BiomeProgressionConfig : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/_Game/Data/BiomeProgression.asset";

        [SerializeField] private List<BiomeGate> gates = new List<BiomeGate>
        {
            new BiomeGate { biome = BiomeId.Starter, requiredCoins = 0 },
            new BiomeGate { biome = BiomeId.FishingCoast, requiredCoins = 1500 },
            new BiomeGate { biome = BiomeId.Mountain, requiredCoins = 5000 },
            new BiomeGate { biome = BiomeId.Snow, requiredCoins = 12000 },
            new BiomeGate { biome = BiomeId.Industrial, requiredCoins = 25000 },
            new BiomeGate { biome = BiomeId.Mystic, requiredCoins = 50000 }
        };

        public IReadOnlyList<BiomeGate> Gates => gates;

        public BiomeGate GetGate(BiomeId biome)
        {
            foreach (var gate in gates)
            {
                if (gate.biome == biome)
                    return gate;
            }

            return null;
        }
    }

    [Serializable]
    public class BiomeGate
    {
        public BiomeId biome;
        public int requiredCoins;
        public BiomeId requiresBiome = BiomeId.Starter;
    }
}
