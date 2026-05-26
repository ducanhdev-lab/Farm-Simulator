using UnityEngine;

namespace IslandHarvest.Game
{
    public class EconomyService : IEconomyService
    {
        private readonly IWalletService wallet;
        private readonly IRemoteConfigService remoteConfig;

        public EconomyService(IWalletService wallet, IRemoteConfigService remoteConfig = null)
        {
            this.wallet = wallet;
            this.remoteConfig = remoteConfig ?? new LocalRemoteConfigService();
        }

        public int GetSellPrice(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0)
                return 0;

            float multiplier = remoteConfig.GetStallPriceMultiplier(item.ItemId, 1f);
            return Mathf.RoundToInt(item.Price * quantity * multiplier);
        }

        public bool TrySellAtStall(ItemData item, int quantity, out int coinsEarned)
        {
            coinsEarned = GetSellPrice(item, quantity);
            if (coinsEarned <= 0)
                return false;

            wallet.Add(coinsEarned);
            GameEventBus.RaiseSoldAtStall(coinsEarned);
            return true;
        }

        public bool TryPurchaseUpgrade(int cost) => wallet.TrySpend(cost);
    }
}
