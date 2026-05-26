using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IslandHarvest.Game
{
    public static class EventTravelService
    {
        public static bool CanTravelTo(EventWorldConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.SceneName))
                return false;

            if (!SaveSystem.IsSceneInBuildSettings(config.SceneName))
                return false;

            var profile = ProfileMigration.LoadOrMigrate();
            return config.IsUnlocked(profile);
        }

        public static void TravelToHome(MonoBehaviour host)
        {
            if (host != null)
                host.StartCoroutine(LoadScene(WorldSceneIds.HomeIsland));
            else
                SceneManager.LoadScene(WorldSceneIds.HomeIsland);
        }

        public static void TravelToEvent(MonoBehaviour host, EventWorldConfig config)
        {
            if (!CanTravelTo(config))
                return;

            GameServices.Instance?.Save?.SaveNow();

            if (host != null)
                host.StartCoroutine(LoadScene(config.SceneName));
            else
                SceneManager.LoadScene(config.SceneName);
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            if (UIManager.Instance != null)
                yield return UIManager.Instance.FadeInScreen();

            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
                yield break;

            op.allowSceneActivation = false;
            while (!op.isDone)
            {
                if (op.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(0.5f);
                    op.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}
