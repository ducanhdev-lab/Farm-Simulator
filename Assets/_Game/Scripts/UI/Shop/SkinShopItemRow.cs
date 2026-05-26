using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IslandHarvest.Game
{
    public class SkinShopItemRow : MonoBehaviour
    {
        static readonly Color RowNormalBg = new Color(1f, 1f, 1f, 0.06f);
        static readonly Color RowEquippedBg = new Color(0.35f, 0.55f, 0.75f, 0.18f);
        static readonly Color EquipButtonColor = new Color(0.22f, 0.72f, 0.38f);
        static readonly Color EquippedButtonColor = new Color(0.38f, 0.42f, 0.48f);
        static readonly Color BuyButtonColor = new Color(0.2f, 0.58f, 0.88f);
        static readonly Color StatusOwnedColor = new Color(0.72f, 0.78f, 0.84f);
        static readonly Color StatusEquippedColor = new Color(0.55f, 0.85f, 0.65f);

        [SerializeField] private Image previewImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionButtonLabel;
        [SerializeField] private Image actionButtonImage;
        [SerializeField] private Image rowBackground;

        [System.Obsolete("Shop is main-menu only. Use SetupForMainMenu.")]
        public void Setup(SkinData skin, PlayerSkinManager skinManager, System.Action onChanged)
        {
            nameText.text = skin.DisplayName;

            if (skin.SkinMaterial != null)
                previewImage.color = skin.SkinMaterial.color;

            bool owned = skinManager.OwnsSkin(skin.SkinId);
            bool equipped = skinManager.EquippedSkinId == skin.SkinId;

            actionButton.onClick.RemoveAllListeners();

            if (equipped)
            {
                priceText.text = "Equipped";
                actionButtonLabel.text = "Equipped";
                actionButton.interactable = false;
                ApplyVisualState(equipped: true, owned: true, canPurchase: false);
                return;
            }

            if (owned)
            {
                priceText.text = "Owned";
                actionButtonLabel.text = "Equip";
                actionButton.interactable = true;
                actionButton.onClick.AddListener(() =>
                {
                    if (skinManager.EquipSkin(skin.SkinId))
                    {
                        AudioManager.Instance?.PlaySFX(AudioID.UI_Accept);
                        onChanged?.Invoke();
                    }
                });
                ApplyVisualState(equipped: false, owned: true, canPurchase: false);
                return;
            }

            if (skin.Price <= 0 || skin.UnlockedByDefault)
            {
                priceText.text = "Free";
                actionButtonLabel.text = "Get";
            }
            else
            {
                priceText.text = skin.Price.ToAbbreviatedString();
                actionButtonLabel.text = "Buy";
            }

            bool canAfford = IslandManager.Instance != null && IslandManager.Instance.Coin >= skin.Price;
            actionButton.interactable = canAfford || skin.Price <= 0;
            ApplyVisualState(equipped: false, owned: false, canPurchase: actionButton.interactable);
            actionButton.onClick.AddListener(() =>
            {
                if (skinManager.TryPurchase(skin))
                {
                    AudioManager.Instance?.PlaySFX(AudioID.UI_Accept);
                    onChanged?.Invoke();
                }
                else
                {
                    AudioManager.Instance?.PlaySFX(AudioID.UI_Decline);
                }
            });
        }

        /// <summary>Main menu shop: buy with save coins, equip owned skins.</summary>
        public void SetupForMainMenu(SkinData skin, string equippedSkinId, int playerCoins, System.Action onChanged)
        {
            nameText.text = skin.DisplayName;

            if (skin.SkinMaterial != null)
                previewImage.color = skin.SkinMaterial.color;

            bool owned = CosmeticsSave.OwnsSkin(skin.SkinId);
            bool equipped = equippedSkinId == skin.SkinId;

            actionButton.onClick.RemoveAllListeners();

            if (equipped)
            {
                priceText.text = "Equipped";
                actionButtonLabel.text = "Equipped";
                actionButton.interactable = false;
                ApplyVisualState(equipped: true, owned: true, canPurchase: false);
                return;
            }

            if (owned)
            {
                priceText.text = "Owned";
                actionButtonLabel.text = "Equip";
                actionButton.interactable = true;
                actionButton.onClick.AddListener(() =>
                {
                    if (CosmeticsSave.TryEquip(skin.SkinId))
                    {
                        AudioManager.Instance?.PlaySFX(AudioID.UI_Accept);
                        onChanged?.Invoke();
                    }
                });
                ApplyVisualState(equipped: false, owned: true, canPurchase: false);
                return;
            }

            if (skin.Price <= 0 || skin.UnlockedByDefault)
            {
                priceText.text = "Free";
                actionButtonLabel.text = "Get";
            }
            else
            {
                priceText.text = skin.Price.ToAbbreviatedString();
                actionButtonLabel.text = "Buy";
            }

            bool canAfford = playerCoins >= skin.Price || skin.Price <= 0;
            actionButton.interactable = canAfford;
            ApplyVisualState(equipped: false, owned: false, canPurchase: canAfford);
            actionButton.onClick.AddListener(() =>
            {
                if (CosmeticsSave.TryPurchaseFromMenu(skin, out _))
                {
                    AudioManager.Instance?.PlaySFX(skin.Price > 0 ? AudioID.Cash : AudioID.UI_Accept);
                    onChanged?.Invoke();
                }
                else
                {
                    AudioManager.Instance?.PlaySFX(AudioID.UI_Decline);
                }
            });
        }

        void ApplyVisualState(bool equipped, bool owned, bool canPurchase)
        {
            if (rowBackground != null)
                rowBackground.color = equipped ? RowEquippedBg : RowNormalBg;

            if (priceText != null)
                priceText.color = equipped ? StatusEquippedColor : StatusOwnedColor;

            var buttonImage = actionButtonImage != null ? actionButtonImage : actionButton?.GetComponent<Image>();
            if (buttonImage == null)
                return;

            if (equipped)
                buttonImage.color = EquippedButtonColor;
            else if (owned)
                buttonImage.color = EquipButtonColor;
            else
                buttonImage.color = canPurchase ? BuyButtonColor : EquippedButtonColor;
        }
    }
}
