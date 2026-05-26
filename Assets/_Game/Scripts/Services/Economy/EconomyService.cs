namespace IslandHarvest.Game
{
    public class EconomyService : IEconomyService
    {
        private readonly IWalletService wallet;

        public EconomyService(IWalletService wallet)
        {
            this.wallet = wallet;
        }

        public int GetSellPrice(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0)
                return 0;

            return item.Price * quantity;
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
