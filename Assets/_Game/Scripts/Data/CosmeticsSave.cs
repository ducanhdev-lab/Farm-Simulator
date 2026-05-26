using System.Collections.Generic;

namespace IslandHarvest.Game
{
    public static class CosmeticsSave
    {
        public const string FileName = "Cosmetics";

        public static CosmeticsData Load()
        {
            var data = SaveSystem.LoadData<CosmeticsData>(FileName);
            if (data == null)
                data = new CosmeticsData();

            EnsureDefaults(data);
            return data;
        }

        public static void Save(CosmeticsData data)
        {
            EnsureDefaults(data);
            SaveSystem.SaveData(data, FileName);
        }

        public static void MergeFromLevelSave(GameData levelData)
        {
            if (levelData == null)
                return;

            var cosmetics = Load();
            bool changed = false;

            if (levelData.ownedSkinIds != null && levelData.ownedSkinIds.Count > 0)
            {
                foreach (string skinId in levelData.ownedSkinIds)
                {
                    if (!cosmetics.ownedSkinIds.Contains(skinId))
                    {
                        cosmetics.ownedSkinIds.Add(skinId);
                        changed = true;
                    }
                }
            }

            MergeIAPProductIds(cosmetics, levelData.completedIAPProductIds, ref changed);

            if (!string.IsNullOrEmpty(cosmetics.equippedSkinId))
                levelData.equippedSkinId = cosmetics.equippedSkinId;
            else if (!string.IsNullOrEmpty(levelData.equippedSkinId))
            {
                cosmetics.equippedSkinId = levelData.equippedSkinId;
                changed = true;
            }

            levelData.ownedSkinIds = new List<string>(cosmetics.ownedSkinIds);

            if (changed)
                Save(cosmetics);
        }

        public static bool OwnsSkin(string skinId) => Load().ownedSkinIds.Contains(skinId);

        public static string EquippedSkinId => Load().equippedSkinId;

        public static bool HasPurchasedIAP(string productId)
        {
            if (string.IsNullOrEmpty(productId))
                return false;

            return Load().completedIAPProductIds.Contains(productId);
        }

        public static bool TryEquip(string skinId)
        {
            var data = Load();
            if (!data.ownedSkinIds.Contains(skinId))
                return false;

            data.equippedSkinId = skinId;
            Save(data);
            return true;
        }

        public static int GetLatestSaveCoins()
        {
            string saveName = SaveSystem.GetLatestSaveFileName();
            if (string.IsNullOrEmpty(saveName))
                return Load().pendingCoins;

            var gameData = SaveSystem.LoadData<GameData>(saveName);
            return gameData?.Coin ?? Load().pendingCoins;
        }

        public static bool TrySetLatestSaveCoins(int coinBalance)
        {
            string saveName = SaveSystem.GetLatestSaveFileName();
            if (string.IsNullOrEmpty(saveName))
                return false;

            var gameData = SaveSystem.LoadData<GameData>(saveName);
            if (gameData == null)
                return false;

            gameData.Coin = coinBalance;
            SaveSystem.SaveData(gameData, saveName);
            return true;
        }

        public static void GrantCoinPack(int amount)
        {
            if (amount <= 0)
                return;

            if (IslandManager.Instance != null)
            {
                IslandManager.Instance.Coin += amount;
                return;
            }

            if (TrySetLatestSaveCoins(GetLatestSaveCoins() + amount))
                return;

            var data = Load();
            data.pendingCoins += amount;
            Save(data);
        }

        public static void ApplyPendingCoinsToGameData(GameData gameData)
        {
            if (gameData == null)
                return;

            var cosmetics = Load();
            if (cosmetics.pendingCoins <= 0)
                return;

            gameData.Coin += cosmetics.pendingCoins;
            cosmetics.pendingCoins = 0;
            Save(cosmetics);
        }

        public static void UnlockSkinFromIAP(string skinId)
        {
            if (string.IsNullOrEmpty(skinId))
                return;

            var data = Load();
            if (!data.ownedSkinIds.Contains(skinId))
                data.ownedSkinIds.Add(skinId);

            data.equippedSkinId = skinId;
            Save(data);
        }

        public static void GrantIAPProduct(IAPProductData product, bool recordProductId)
        {
            if (product == null)
                return;

            switch (product.Kind)
            {
                case IAPProductKind.CoinPack:
                    GrantCoinPack(product.CoinAmount);
                    break;

                case IAPProductKind.PremiumSkin:
                    if (!string.IsNullOrEmpty(product.LinkedSkinId))
                        UnlockSkinFromIAP(product.LinkedSkinId);

                    if (recordProductId)
                    {
                        var data = Load();
                        if (!data.completedIAPProductIds.Contains(product.StoreProductId))
                        {
                            data.completedIAPProductIds.Add(product.StoreProductId);
                            Save(data);
                        }
                    }
                    break;
            }

            IslandManager.Instance?.SyncCosmeticsToActiveSave();
        }

        public static bool TryPurchase(SkinData skin, int playerCoins, out int newCoinBalance)
        {
            newCoinBalance = playerCoins;
            if (skin == null || skin.PurchaseType == SkinPurchaseType.IapOnly)
                return false;

            var data = Load();
            if (data.ownedSkinIds.Contains(skin.SkinId))
            {
                data.equippedSkinId = skin.SkinId;
                Save(data);
                return true;
            }

            if (skin.Price > 0 && playerCoins < skin.Price)
                return false;

            if (skin.Price > 0)
                newCoinBalance = playerCoins - skin.Price;

            data.ownedSkinIds.Add(skin.SkinId);
            data.equippedSkinId = skin.SkinId;
            Save(data);
            return true;
        }

        public static bool TryPurchaseFromMenu(SkinData skin, out int newCoinBalance)
        {
            newCoinBalance = GetLatestSaveCoins();
            if (!TryPurchase(skin, newCoinBalance, out newCoinBalance))
                return false;

            if (skin.Price > 0)
                TrySetLatestSaveCoins(newCoinBalance);

            return true;
        }

        public static void SyncToLevelSave(GameData levelData)
        {
            if (levelData == null) return;

            var cosmetics = Load();
            levelData.ownedSkinIds = new List<string>(cosmetics.ownedSkinIds);
            levelData.equippedSkinId = cosmetics.equippedSkinId;
            levelData.completedIAPProductIds = new List<string>(cosmetics.completedIAPProductIds);
        }

        private static void MergeIAPProductIds(CosmeticsData cosmetics, List<string> levelIds, ref bool changed)
        {
            if (levelIds == null || levelIds.Count == 0)
                return;

            if (cosmetics.completedIAPProductIds == null)
                cosmetics.completedIAPProductIds = new List<string>();

            foreach (string id in levelIds)
            {
                if (!cosmetics.completedIAPProductIds.Contains(id))
                {
                    cosmetics.completedIAPProductIds.Add(id);
                    changed = true;
                }
            }
        }

        private static void EnsureDefaults(CosmeticsData data)
        {
            if (data.ownedSkinIds == null)
                data.ownedSkinIds = new List<string>();

            if (!data.ownedSkinIds.Contains(PlayerSkinManager.DefaultSkinId))
                data.ownedSkinIds.Add(PlayerSkinManager.DefaultSkinId);

            if (string.IsNullOrEmpty(data.equippedSkinId))
                data.equippedSkinId = PlayerSkinManager.DefaultSkinId;

            if (data.completedIAPProductIds == null)
                data.completedIAPProductIds = new List<string>();
        }
    }
}
