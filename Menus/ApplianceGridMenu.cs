using System;
using System.Collections.Generic;
using Kitchen.Modules;
using KitchenData;
using UnityEngine;

namespace ThatsWhatINeed.Menus
{
    public class ApplianceGridMenu : GridMenu<GridItemOption>
    {
        public ApplianceGridMenu(List<GridItemOption> items, Transform container, int player, bool has_back) : base(items, container, player, has_back)
        {
        }

        protected override int ColumnLength => 5;

        protected override void SetupElement(GridItemOption item, GridMenuElement element)
        {
            element.Set(item);
        }

        protected override void OnSelect(GridItemOption item)
        {
            item.DoCallback();
        }
    }

    [Serializable]
    public struct GridItemOption : IGridItem
    {
        public readonly int ActionID;
        public Texture2D icon;
        public bool isSelected;
        private Action<int> SelectCallback;

        public GridItemOption(int ActionID, Action<int> callback, bool isSelected, Texture2D icon = null)
        {
            this.ActionID = ActionID;
            this.icon = icon;
            this.isSelected = isSelected;
            SelectCallback = callback;
        }

        public int SnapshotKey => ActionID;

        public Texture2D GetSnapshot()
        {
            if (GameData.Main.TryGet(ActionID, out Appliance appliance))
            {
                if (isSelected)
                    return Utility.PrefabSnapshot.GetSelectedApplianceSnapshot(GameData.Main.Get<Appliance>(ActionID).Prefab);
                return Utility.PrefabSnapshot.GetApplianceSnapshot(GameData.Main.Get<Appliance>(ActionID).Prefab);
            }
            
            return icon;
        }

        public void DoCallback()
        {
            SelectCallback?.Invoke(ActionID);
        }
    }
}
