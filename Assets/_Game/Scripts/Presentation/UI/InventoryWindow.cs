using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IslandHarvest.Game
{
    public class InventoryWindow : WindowBase
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private Transform gridContainer;
        [SerializeField] private ItemInfo itemSlotPrefab;
        [SerializeField] private TMP_Dropdown categoryFilter;

        private readonly List<ItemInfo> slots = new List<ItemInfo>();
        private Inventory boundInventory;
        private ResourceCategory? filterCategory;

        protected override void Awake()
        {
            base.Awake();
            Hide();

            if (toggleButton != null)
                toggleButton.onClick.AddListener(Toggle);

            if (categoryFilter != null)
            {
                categoryFilter.ClearOptions();
                categoryFilter.AddOptions(new List<string> { "All", "Crop", "Animal", "Fish", "Processed", "Other" });
                categoryFilter.onValueChanged.AddListener(OnFilterChanged);
            }
        }

        public void Bind(Inventory inventory)
        {
            boundInventory = inventory;
            Refresh();
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        public override void Show()
        {
            base.Show();
            Refresh();
        }

        private void OnFilterChanged(int index)
        {
            filterCategory = index switch
            {
                1 => ResourceCategory.Crop,
                2 => ResourceCategory.Animal,
                3 => ResourceCategory.Fish,
                4 => ResourceCategory.Processed,
                5 => ResourceCategory.Other,
                _ => null
            };
            Refresh();
        }

        public void Refresh()
        {
            if (boundInventory == null || itemSlotPrefab == null || gridContainer == null)
                return;

            var items = boundInventory.Items;
            if (filterCategory.HasValue)
                items = items.Where(i => i.Data != null && i.Data.Category == filterCategory.Value).ToList();

            while (slots.Count > items.Count)
            {
                var slot = slots[slots.Count - 1];
                slots.Remove(slot);
                Destroy(slot.gameObject);
            }

            while (slots.Count < items.Count)
            {
                var slot = Instantiate(itemSlotPrefab, gridContainer);
                slots.Add(slot);
            }

            for (int i = 0; i < items.Count; i++)
            {
                slots[i].Init(items[i].Data);
                slots[i].UpdateInfo(items[i]);
            }
        }
    }
}
