using UnityEngine;

namespace IslandHarvest.Game
{
    [CreateAssetMenu()]
    public class CropData : ItemData
    {
        [Header("Crop Prefab References")]
        [SerializeField] private GameObject sproutPrefab;
        [SerializeField] private GameObject plantPrefab;

        public GameObject SproutPrefab => sproutPrefab;
        public GameObject PlantPrefab => plantPrefab;
    }
}
