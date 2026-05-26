using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IslandHarvest.Game
{
    public class IAPShopItemRow : MonoBehaviour
    {
        [SerializeField] private Image previewImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionButtonLabel;

        private IAPProductData product;
        private System.Action onChanged;

        public void Setup(
            IAPProductData iapProduct,
            string equippedSkinId,
            System.Action refreshCallback)
        {
            product = iapProduct;
            onChanged = refreshCallback;

            nameText.text = iapProduct.DisplayName;

            if (iconImage != null)
            {
                iconImage.enabled = iapProduct.Icon != null;
                if (iapProduct.Icon != null)
                    iconImage.sprite = iapProduct.Icon;
            }

            if (iapProduct.Kind == IAPProductKind.PremiumSkin)
                SetupPremiumSkin(iapProduct, equippedSkinId);
            else
                SetupCoinPack(iapProduct);
        }

        private void SetupCoinPack(IAPProductData iapProduct)
        {
            if (previewImage != null)
                previewImage.color = new Color(1f, 0.85f, 0.2f);

            priceText.text = IAPService.Instance != null
                ? IAPService.Instance.GetLocalizedPrice(iapProduct.StoreProductId)
                : $"{iapProduct.CoinAmount.ToAbbreviatedString()} coins";

            actionButton.onClick.RemoveAllListeners();
            actionButtonLabel.text = "Buy";
            bool pending = IAPService.Instance != null && IAPService.Instance.IsPurchasePending;
            actionButton.interactable = !pending;
            actionButton.onClick.AddListener(() =>
            {
                IAPService.EnsureInstance()?.Purchase(iapProduct.StoreProductId);
            });
        }

        private void SetupPremiumSkin(IAPProductData iapProduct, string equippedSkinId)
        {
            SkinData skin = FindSkin(iapProduct.LinkedSkinId);
            if (skin != null && previewImage != null && skin.SkinMaterial != null)
                previewImage.color = skin.SkinMaterial.color;

            bool owned = CosmeticsSave.OwnsSkin(iapProduct.LinkedSkinId);
            bool equipped = equippedSkinId == iapProduct.LinkedSkinId;
            bool purchased = CosmeticsSave.HasPurchasedIAP(iapProduct.StoreProductId) || owned;

            actionButton.onClick.RemoveAllListeners();

            if (equipped)
            {
                priceText.text = "Equipped";
                actionButtonLabel.text = "Equipped";
                actionButton.interactable = false;
                return;
            }

            if (purchased)
            {
                priceText.text = "Owned";
                actionButtonLabel.text = "Equip";
                actionButton.interactable = true;
                actionButton.onClick.AddListener(() =>
                {
                    if (CosmeticsSave.TryEquip(iapProduct.LinkedSkinId))
                    {
                        AudioManager.Instance?.PlaySFX(AudioID.UI_Accept);
                        onChanged?.Invoke();
                    }
                });
                return;
            }

            priceText.text = IAPService.Instance != null
                ? IAPService.Instance.GetLocalizedPrice(iapProduct.StoreProductId)
                : "IAP";

            actionButtonLabel.text = "Buy";
            bool pending = IAPService.Instance != null && IAPService.Instance.IsPurchasePending;
            actionButton.interactable = !pending;
            actionButton.onClick.AddListener(() =>
            {
                IAPService.EnsureInstance()?.Purchase(iapProduct.StoreProductId);
            });
        }

        private static SkinData FindSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId))
                return null;

            var skins = Resources.LoadAll<SkinData>("Skins");
            foreach (var skin in skins)
            {
                if (skin != null && skin.SkinId == skinId)
                    return skin;
            }

            return null;
        }
    }
}
