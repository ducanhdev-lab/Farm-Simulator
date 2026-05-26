using UnityEngine;

namespace IslandHarvest.Game
{
    [CreateAssetMenu(fileName = "EventWorld", menuName = "Island Harvest/Event World")]
    public class EventWorldConfig : ScriptableObject
    {
        [SerializeField] private string eventId;
        [SerializeField] private string sceneName;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool alwaysAvailable = true;
        [SerializeField] private int requiredHomeCoins;

        public string EventId => eventId;
        public string SceneName => sceneName;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public bool AlwaysAvailable => alwaysAvailable;
        public int RequiredHomeCoins => requiredHomeCoins;

        public bool IsUnlocked(PlayerProfile profile)
        {
            if (alwaysAvailable)
                return true;

            int homeCoins = profile?.homeWorld?.Coin ?? 0;
            return homeCoins >= requiredHomeCoins;
        }

        public void ConfigureRuntime(string id, string scene, string title, bool unlocked = true, int coinGate = 0)
        {
            eventId = id;
            sceneName = scene;
            displayName = title;
            alwaysAvailable = unlocked;
            requiredHomeCoins = coinGate;
        }
    }
}
