using System;

namespace IslandHarvest.Game
{
    public interface IWalletService
    {
        int Balance { get; }
        event Action<int> OnBalanceChanged;

        void Bind(GameData worldData);
        void SetBalance(int amount);
        bool TrySpend(int amount);
        void Add(int amount);
    }
}
