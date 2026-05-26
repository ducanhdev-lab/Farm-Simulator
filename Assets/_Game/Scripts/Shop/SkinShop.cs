using UnityEngine;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Legacy in-world shop trigger. Purchases and IAP are main-menu only
    /// (<see cref="GameShopWindow.ShowForMainMenu"/>).
    /// </summary>
    [System.Obsolete("Skin shop is main-menu only. Remove SkinShop objects from level scenes.")]
    public class SkinShop : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Reset()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
        }
#endif
    }

}
