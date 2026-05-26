using UnityEngine;

namespace IslandHarvest.Game
{
    [CreateAssetMenu(fileName = "Building", menuName = "Island Harvest/Building")]
    public class BuildingData : ScriptableObject
    {
        [SerializeField] private string buildingId;
        [SerializeField] private string displayName;
        [SerializeField] private BiomeId requiredBiome = BiomeId.Starter;
        [SerializeField] private int unlockCost;
        [SerializeField] private RecipeData defaultRecipe;
        [SerializeField] private GameObject prefab;

        public string BuildingId => buildingId;
        public string DisplayName => displayName;
        public BiomeId RequiredBiome => requiredBiome;
        public int UnlockCost => unlockCost;
        public RecipeData DefaultRecipe => defaultRecipe;
        public GameObject Prefab => prefab;
    }
}
