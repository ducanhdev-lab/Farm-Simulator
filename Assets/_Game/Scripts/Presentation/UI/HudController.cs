using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleInputNamespace;

namespace IslandHarvest.Game
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private Joystick joystick;
        [SerializeField] private List<GameObject> hudElements = new List<GameObject>();
        [SerializeField] private Button activityButton;
        [SerializeField] private Button farmingButton;
        [SerializeField] private Sprite[] farmingIcons;
        [SerializeField] private ItemInfo itemInfoPrefab;
        [SerializeField] private Transform itemInfoContainer;

        private readonly List<ItemInfo> itemInfos = new List<ItemInfo>();

        public Joystick Joystick => joystick;

        public void ToggleHUD(bool active)
        {
            foreach (var hud in hudElements)
            {
                if (hud != null)
                    hud.SetActive(active);
            }
        }

        public void SetActiveJoystick(bool active)
        {
            if (joystick != null)
                joystick.gameObject.SetActive(active);
        }

        public void ReleaseJoystick()
        {
            if (joystick != null)
                joystick.OnPointerUp(null);
        }

        public void ToggleFarmingButton(Farm.State farmState, System.Action farmingAction)
        {
            if (farmingButton == null)
                return;

            var iconImage = farmingButton.transform.GetChild(0).GetComponent<Image>();
            iconImage.sprite = farmingIcons[(int)farmState];
            farmingButton.onClick.RemoveAllListeners();
            farmingButton.onClick.AddListener(() =>
            {
                farmingAction?.Invoke();
                farmingButton.gameObject.SetActive(false);
            });
            farmingButton.gameObject.SetActive(true);
        }

        public void DisableFarmingButton()
        {
            if (farmingButton != null)
                farmingButton.gameObject.SetActive(false);
        }

        public void ToggleActivityButton(Sprite icon, System.Action activity)
        {
            if (activityButton == null)
                return;

            activityButton.gameObject.SetActive(activity != null);
            if (activity == null)
                return;

            activityButton.transform.GetChild(0).GetComponent<Image>().sprite = icon;
            activityButton.onClick.RemoveAllListeners();
            activityButton.onClick.AddListener(() => activity.Invoke());
        }

        public void UpdateItemInfos(List<Item> items)
        {
            if (itemInfoPrefab == null || itemInfoContainer == null)
                return;

            while (itemInfos.Count > items.Count)
            {
                var last = itemInfos[itemInfos.Count - 1];
                itemInfos.Remove(last);
                Destroy(last.gameObject);
            }

            while (itemInfos.Count < items.Count)
            {
                var info = Instantiate(itemInfoPrefab, itemInfoContainer);
                itemInfos.Add(info);
            }

            for (int i = 0; i < items.Count; i++)
            {
                itemInfos[i].Init(items[i].Data);
                itemInfos[i].UpdateInfo(items[i]);
            }
        }
    }
}
