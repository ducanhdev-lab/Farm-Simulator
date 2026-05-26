using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    public class PanelStack : MonoBehaviour
    {
        [SerializeField] private List<WindowBase> panels = new List<WindowBase>();

        private WindowBase current;

        public void Register(WindowBase panel)
        {
            if (panel != null && !panels.Contains(panel))
                panels.Add(panel);
        }

        public void Show(WindowBase panel)
        {
            if (panel == null)
                return;

            current?.Hide();
            current = panel;
            panel.Show();
        }

        public void HideAll()
        {
            foreach (var panel in panels)
                panel?.Hide();

            current = null;
        }
    }
}
