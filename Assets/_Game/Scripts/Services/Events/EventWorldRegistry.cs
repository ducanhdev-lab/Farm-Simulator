using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class EventWorldRegistry
    {
        private static List<EventWorldConfig> cached;

        public static IReadOnlyList<EventWorldConfig> GetAll()
        {
            if (cached == null)
                ReloadFromResources();

            return cached;
        }

        public static void ApplyLiveEvents(LiveEventData[] events)
        {
            if (events == null || events.Length == 0)
                return;

            cached ??= new List<EventWorldConfig>();
            cached.Clear();

            foreach (var live in events)
            {
                if (live == null || string.IsNullOrEmpty(live.eventId))
                    continue;

                var config = ScriptableObject.CreateInstance<EventWorldConfig>();
                config.name = live.displayName;
                config.ConfigureRuntime(
                    live.eventId,
                    live.sceneName,
                    live.displayName,
                    live.alwaysAvailable,
                    live.requiredHomeCoins);
                cached.Add(config);
            }
        }

        public static void ReloadFromResources()
        {
            cached = new List<EventWorldConfig>(Resources.LoadAll<EventWorldConfig>("EventWorlds"));

            if (cached.Count == 0)
                cached.AddRange(CreateDefaultConfigs());
        }

        private static IEnumerable<EventWorldConfig> CreateDefaultConfigs()
        {
            yield return CreateRuntimeConfig("event_snow", WorldSceneIds.SnowEvent, "Snow Festival", true);
            yield return CreateRuntimeConfig("event_coast", WorldSceneIds.CoastEvent, "Fishing Coast", true);
        }

        private static EventWorldConfig CreateRuntimeConfig(string eventId, string scene, string title, bool alwaysOpen)
        {
            var config = ScriptableObject.CreateInstance<EventWorldConfig>();
            config.name = title;
            config.ConfigureRuntime(eventId, scene, title, alwaysOpen);
            return config;
        }
    }
}
