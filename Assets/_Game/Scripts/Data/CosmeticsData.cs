using System.Collections.Generic;

namespace IslandHarvest.Game
{
    [System.Serializable]
    public class CosmeticsData
    {
        public List<string> ownedSkinIds = new List<string> { PlayerSkinManager.DefaultSkinId };
        public string equippedSkinId = PlayerSkinManager.DefaultSkinId;
        public List<string> completedIAPProductIds = new List<string>();
        public int pendingCoins;
    }
}
