using System.Collections.Generic;
using Kitchen;
using Kitchen.Modules;
using KitchenLib;
using KitchenLib.Preferences;
using UnityEngine;

namespace ThatsWhatINeed.Menus
{
    public class PreferenceMenu<T> : KLMenu<T>
    {
        public PreferenceMenu(Transform container, ModuleList module_list) : base(container, module_list)
        {
        }

        private Option<int> notificationSound = new Option<int>(new List<int> { 0, 1, 2 }, Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Value, new List<string> { "None", "Bell Ring", "Scoom Mode" });
        private Option<float> bellVolume = new Option<float>(new List<float> { 0.0f, 0.25f, 0.50f, 0.75f, 1.0f }, Mod.Manager.GetPreference<PreferenceFloat>("bellVolume").Value, new List<string> { "0%", "25%", "50%", "75%", "100%" });
        private Option<float> scoomVolume = new Option<float>(new List<float> { 0.0f, 0.25f, 0.50f, 0.75f, 1.0f }, Mod.Manager.GetPreference<PreferenceFloat>("scoomVolume").Value, new List<string> { "0%", "25%", "50%", "75%", "100%" });
        private Option<bool> shouldWarningFlash = new Option<bool>(new List<bool> { true, false }, Mod.Manager.GetPreference<PreferenceBool>("shouldWarningFlash").Value, new List<string> { "Enabled", "Disabled" });
        private Option<float> warningPercentage = new Option<float>(new List<float> { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f }, Mod.Manager.GetPreference<PreferenceFloat>("warningPercentage").Value, new List<string> { "0%", "25%", "50%", "75%", "100%" });

        public override void Setup(int player_id)
        {
            AddLabel("Notification Sound");
            New<SpacerElement>(true);
            AddSelect(notificationSound);
            notificationSound.OnChanged += delegate (object _, int result)
            {
                Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Set(result);
                Mod.Manager.Save();
            };
            New<SpacerElement>(true);
            
            
            AddLabel("Flashing Warning Indicator");
            New<SpacerElement>(true);
            AddSelect(shouldWarningFlash);
            shouldWarningFlash.OnChanged += delegate (object _, bool result)
            {
                Mod.Manager.GetPreference<PreferenceBool>("shouldWarningFlash").Set(result);
                Mod.Manager.Save();
            };
            New<SpacerElement>(true);
            
            
            AddLabel("Warning Bar Percentage");
            New<SpacerElement>(true);
            AddSelect(warningPercentage);
            warningPercentage.OnChanged += delegate (object _, float result)
            {
                Mod.Manager.GetPreference<PreferenceFloat>("warningPercentage").Set(result);
                Mod.Manager.Save();
            };
            New<SpacerElement>(true);
            
            
            AddLabel("Bell Volume");
            New<SpacerElement>(true);
            AddSelect(bellVolume);
            bellVolume.OnChanged += delegate (object _, float result)
            {
                Mod.Manager.GetPreference<PreferenceFloat>("bellVolume").Set(result);
                Mod.Manager.Save();
            };
            New<SpacerElement>(true);
            
            
            AddLabel("Scoom Volume");
            New<SpacerElement>(true);
            AddSelect(scoomVolume);
            scoomVolume.OnChanged += delegate (object _, float result)
            {
                Mod.Manager.GetPreference<PreferenceFloat>("scoomVolume").Set(result);
                Mod.Manager.Save();
            };
            
            New<SpacerElement>(true);
            New<SpacerElement>(true);
            
            AddButton(base.Localisation["MENU_BACK_SETTINGS"], delegate(int i)
            {
                Mod.Manager.Save();
                RequestPreviousMenu();
            });
        }
    }
}