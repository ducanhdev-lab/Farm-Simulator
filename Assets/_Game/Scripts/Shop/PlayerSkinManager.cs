using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IslandHarvest.Game
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerSkinManager : MonoBehaviour
    {
        public const string DefaultSkinId = "skin_yellow";

        [SerializeField, Tooltip("Fallback catalog when Resources/Skins is empty.")]
        private SkinData[] skinCatalog;

        private PlayerController playerController;
        private readonly Dictionary<string, SkinData> skinLookup = new Dictionary<string, SkinData>();

        public string EquippedSkinId { get; private set; }

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            BuildLookup();
        }

        private void Start()
        {
            ApplyFromCosmeticsSave();

            if (IslandManager.Instance != null)
            {
                IslandManager.Instance.OnSaveGame += SyncToLevelSave;
                PushCosmeticsToIslandManager();
                ApplyFromCosmeticsSave();
            }
        }

        private void OnDestroy()
        {
            if (IslandManager.Instance != null)
                IslandManager.Instance.OnSaveGame -= SyncToLevelSave;
        }

        private void BuildLookup()
        {
            skinLookup.Clear();

            var skins = skinCatalog != null && skinCatalog.Length > 0
                ? skinCatalog
                : Resources.LoadAll<SkinData>("Skins");

            foreach (var skin in skins)
            {
                if (skin == null) continue;
                skinLookup[skin.SkinId] = skin;
            }

            foreach (var skin in skinLookup.Values)
            {
                if (skin.UnlockedByDefault)
                {
                    var data = CosmeticsSave.Load();
                    if (!data.ownedSkinIds.Contains(skin.SkinId))
                    {
                        data.ownedSkinIds.Add(skin.SkinId);
                        CosmeticsSave.Save(data);
                    }
                }
            }
        }

        private void ApplyFromCosmeticsSave()
        {
            ApplySkinById(CosmeticsSave.EquippedSkinId);
        }

        private void PushCosmeticsToIslandManager()
        {
            var manager = IslandManager.Instance;
            var cosmetics = CosmeticsSave.Load();
            manager.OwnedSkinIds = new List<string>(cosmetics.ownedSkinIds);
            manager.EquippedSkinId = cosmetics.equippedSkinId;
        }

        private void SyncToLevelSave(GameData gameData)
        {
            CosmeticsSave.SyncToLevelSave(gameData);
        }

        public bool OwnsSkin(string skinId) => CosmeticsSave.OwnsSkin(skinId);

        public bool TryPurchase(SkinData skin)
        {
            if (skin == null)
                return false;

            if (OwnsSkin(skin.SkinId))
                return EquipSkin(skin.SkinId);

            int coins = IslandManager.Instance != null
                ? IslandManager.Instance.Coin
                : CosmeticsSave.GetLatestSaveCoins();

            if (!CosmeticsSave.TryPurchase(skin, coins, out int newBalance))
                return false;

            if (IslandManager.Instance != null)
                IslandManager.Instance.Coin = newBalance;
            else
                CosmeticsSave.TrySetLatestSaveCoins(newBalance);

            AudioManager.Instance?.PlaySFX(AudioID.Cash);
            ApplySkinById(skin.SkinId);
            return true;
        }

        public bool EquipSkin(string skinId)
        {
            if (!OwnsSkin(skinId) && skinId != DefaultSkinId)
                return false;

            if (!CosmeticsSave.TryEquip(skinId))
                return false;

            ApplySkinById(skinId);

            if (IslandManager.Instance != null)
                IslandManager.Instance.EquippedSkinId = skinId;

            return true;
        }

        public void ApplySkinById(string skinId)
        {
            if (!skinLookup.TryGetValue(skinId, out SkinData skin))
                skin = skinLookup.Values.FirstOrDefault(s => s.SkinId == DefaultSkinId);

            if (skin == null || skin.SkinMaterial == null)
                return;

            EquippedSkinId = skin.SkinId;
            playerController.ApplySkinMaterials(skin.SkinMaterial, skin.TransparentMaterial);
        }

    }
}
