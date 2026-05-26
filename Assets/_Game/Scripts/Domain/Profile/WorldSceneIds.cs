namespace IslandHarvest.Game
{
    public static class WorldSceneIds
    {
        public const string HomeIsland = "Level01";
        public const string SnowEvent = "Level02";
        public const string CoastEvent = "Level03";
        public const string ProfileFileName = "player_profile";

        public static bool IsHomeScene(string sceneName) =>
            sceneName == HomeIsland;

        public static bool IsEventScene(string sceneName) =>
            sceneName == SnowEvent || sceneName == CoastEvent;

        public static string GetEventIdForScene(string sceneName) => sceneName switch
        {
            SnowEvent => "event_snow",
            CoastEvent => "event_coast",
            _ => null
        };
    }
}
