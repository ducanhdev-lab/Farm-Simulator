using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class EventWorldRegistry
    {
        private static List<EventWorldConfig> cached;

        public static IReadOnlyList<EventWorldConfig> GetAll()
        {
            if (cached != null)
                return cached;

            cached = new List<EventWorldConfig>(Resources.LoadAll<EventWorldConfig>("EventWorlds"));

            if (cached.Count == 0)
                cached.AddRange(CreateDefaultConfigs());

            return cached;
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
