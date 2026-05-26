using UnityEngine;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Gates scene content by logical biome. Assign gated roots (props, stations) in the inspector.
    /// </summary>
    public class BiomeZone : MonoBehaviour
    {
        [SerializeField] private BiomeId biome = BiomeId.Starter;
        [SerializeField] private GameObject[] gatedRoots;
        [SerializeField] private bool hideWhenLocked = true;

        public BiomeId Biome => biome;

        public void Refresh(IWorldExpansionService world, GameData data)
        {
            if (world == null || data == null)
                return;

            bool unlocked = world.IsBiomeUnlocked(data, biome);
            var roots = ResolveGatedRoots();

            foreach (var root in roots)
            {
                if (root == null)
                    continue;

                if (hideWhenLocked)
                    root.SetActive(unlocked);
            }
        }

        private GameObject[] ResolveGatedRoots()
        {
            if (gatedRoots != null && gatedRoots.Length > 0)
                return gatedRoots;

            var props = GetComponentsInChildren<IProp>(true);
            var roots = new GameObject[props.Length];
            for (int i = 0; i < props.Length; i++)
                roots[i] = (props[i] as MonoBehaviour)?.gameObject;

            return roots;
        }
    }
}
