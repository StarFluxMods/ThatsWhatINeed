using System;
using System.Collections.Generic;
using Kitchen.Modules;
using KitchenData;
using UnityEngine;

namespace ThatsWhatINeed.Menus
{
    public class ApplianceGridConfig : GridMenuConfig
    {
        public override GridMenu Instantiate(Transform container, int player, bool has_back)
        {
            return new ApplianceGridMenu(new List<GridItemOption>(), container, player, has_back);
        }

        private static Texture2D empty;
        
        public virtual ApplianceGridMenu Instantiate(Action<int> callback, Transform container, int player, bool has_back, List<int> AvailableOptions, List<int> MarkedOptions, int offset)
        {
            if (empty == null)
            {
                empty = new Texture2D(1, 1);
                empty.SetPixel(0, 0, new Color(0, 0, 0, 0));
                empty.Apply();
            }

            List<GridItemOption> gridAppliances = new List<GridItemOption>();

            if (!has_back)
                gridAppliances.Add(new GridItemOption(0, callback, false, Mod.Bundle.LoadAsset<Texture2D>("menu_left")));
            
            // gridAppliances.Add(new GridItemOption(1, callback, false, Mod.Bundle.LoadAsset<Texture2D>("search")));
            
            gridAppliances.Add(new GridItemOption(2, callback, false, empty)); // Spacer (Only for the first page)
            
            gridAppliances.Add(new GridItemOption(2, callback, false, empty)); // Spacer (Only for the first page)
            
            if ((offset + 16) < AvailableOptions.Count)
                gridAppliances.Add(new GridItemOption(3, callback, false, Mod.Bundle.LoadAsset<Texture2D>("menu_right")));
            else
                gridAppliances.Add(new GridItemOption(4, callback, false, empty)); // Spacer (Only for the last page)
            
            for (int i = offset; i < offset + 16; i++)
            {
                if (i < AvailableOptions.Count)
                {
                    bool found = false;
                    for (int x = 0; x < MarkedOptions.Count; x++)
                    {
                        if (MarkedOptions[x] == AvailableOptions[i])
                        {
                            found = true;
                            break;
                        }
                    }
                    gridAppliances.Add(new GridItemOption(AvailableOptions[i], callback, found, Utility.PrefabSnapshot.GetApplianceSnapshot(GameData.Main.Get<Appliance>(AvailableOptions[i]).Prefab)));
                }
                else
                {
                    break;
                }
            }
            
            return new ApplianceGridMenu(gridAppliances, container, player, has_back);
        }
    }
}
