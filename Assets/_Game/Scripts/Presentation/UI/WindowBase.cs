using UnityEngine;

namespace IslandHarvest.Game
{
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] protected GameObject root;

        public bool IsVisible => root != null ? root.activeSelf : gameObject.activeSelf;

        protected virtual void Awake()
        {
            if (root == null)
                root = gameObject;
        }

        public virtual void Show()
        {
            if (root != null)
                root.SetActive(true);
        }

        public virtual void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
