using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    [Serializable]
    public class RemoteBalanceConfig
    {
        public int version;
        public int islandBasePrice;
        public StallMultiplierEntry[] stallMultipliers;
    }

    [Serializable]
    public class StallMultiplierEntry
    {
        public string itemId;
        public float multiplier;
    }

    /// <summary>
    /// Fetches GET /config/balance from API; falls back to local defaults when offline.
    /// </summary>
    public class ApiRemoteConfigService : IRemoteConfigService
    {
        private readonly LocalRemoteConfigService fallback;
        private readonly Dictionary<string, float> stallMultipliers = new Dictionary<string, float>();

        private int islandBasePrice = 100;
        private bool isReady;

        public ApiRemoteConfigService(LocalRemoteConfigService fallback, int defaultIslandPrice = 100)
        {
            this.fallback = fallback ?? new LocalRemoteConfigService();
            islandBasePrice = defaultIslandPrice;
        }

        public bool IsReady => isReady;

        public float GetStallPriceMultiplier(string itemId, float defaultMultiplier)
        {
            if (!string.IsNullOrEmpty(itemId) && stallMultipliers.TryGetValue(itemId, out float mult))
                return mult;

            return fallback.GetStallPriceMultiplier(itemId, defaultMultiplier);
        }

        public int GetIslandBasePrice(int defaultPrice) =>
            isReady ? islandBasePrice : fallback.GetIslandBasePrice(defaultPrice);

        public IEnumerator FetchFromApi(ApiClient client)
        {
            if (client == null)
                yield break;

            bool done = false;
            ApiResult result = default;

            yield return client.Get("/config/balance", r => { result = r; done = true; });

            while (!done)
                yield return null;

            if (!result.Success || string.IsNullOrEmpty(result.Body))
                yield break;

            var config = JsonUtility.FromJson<RemoteBalanceConfig>(result.Body);
            if (config == null)
                yield break;

            Apply(config);
        }

        private void Apply(RemoteBalanceConfig config)
        {
            stallMultipliers.Clear();

            if (config.stallMultipliers != null)
            {
                foreach (var entry in config.stallMultipliers)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.itemId))
                        stallMultipliers[entry.itemId] = entry.multiplier;
                }
            }

            if (config.islandBasePrice > 0)
                islandBasePrice = config.islandBasePrice;

            isReady = true;
            Debug.Log($"Remote config loaded (v{config.version}).");
        }
    }
}
