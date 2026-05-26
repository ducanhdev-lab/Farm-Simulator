using UnityEngine;
using UnityEngine.EventSystems;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Minimap entry for seasonal / event maps (Level02, Level03).
    /// </summary>
    public class EventMapSelect : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private EventWorldConfig eventConfig;

        private void Start()
        {
            if (eventConfig == null)
            {
                int index = transform.GetSiblingIndex();
                var events = EventWorldRegistry.GetAll();
                if (index < events.Count)
                    eventConfig = events[index];
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventConfig == null)
            {
                AudioManager.Instance.PlaySFX(AudioID.UI_Decline);
                return;
            }

            if (!EventTravelService.CanTravelTo(eventConfig))
            {
                AudioManager.Instance.PlaySFX(AudioID.UI_Decline);
                return;
            }

            AudioManager.Instance.PlaySFX(AudioID.UI_Accept);
            EventTravelService.TravelToEvent(this, eventConfig);
        }
    }
}
