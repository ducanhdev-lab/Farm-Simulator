using System;

namespace IslandHarvest.Game
{
    public class WalletService : IWalletService
    {
        private GameData worldData;

        public int Balance => worldData?.Coin ?? 0;
        public event Action<int> OnBalanceChanged;

        public void Bind(GameData data) => worldData = data;

        public void SetBalance(int amount)
        {
            if (worldData == null)
                return;

            worldData.Coin = amount;
            OnBalanceChanged?.Invoke(amount);
        }

        public void Add(int amount)
        {
            if (worldData == null || amount == 0)
                return;

            SetBalance(worldData.Coin + amount);
        }

        public bool TrySpend(int amount)
        {
            if (worldData == null || amount < 0 || worldData.Coin < amount)
                return false;

            SetBalance(worldData.Coin - amount);
            return true;
        }
    }
}
