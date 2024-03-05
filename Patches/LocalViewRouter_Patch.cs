using System.Reflection;
using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;
using ThatsWhatINeed.Menus;
using ThatsWhatINeed.Views;
using UnityEngine;

namespace ThatsWhatINeed.Patches
{
    [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
    public class LocalViewRouter_Patch
    {
        public static GameObject container;
        public static GameObject result;
        
        public static FieldInfo _AssetDirectory = AccessTools.Field(typeof(LocalViewRouter), "AssetDirectory");
        public static FieldInfo f_Container = typeof(CostumeChangeIndicator).GetField("Container", BindingFlags.NonPublic | BindingFlags.Instance);
		
        static bool Prefix(LocalViewRouter __instance, ViewType view_type, ref GameObject __result)
        {
            if (view_type != (ViewType)VariousUtils.GetID("com.starfluxgames.thatswhatineed.appliancegridview")) return true;
            
            if (container == null)
            {
                container = new GameObject("temp");
                container.SetActive(false);
            }
            
            if (result == null)
            {
                AssetDirectory AssetDirectory = (AssetDirectory)_AssetDirectory.GetValue(__instance);
                result = GameObject.Instantiate(AssetDirectory.ViewPrefabs[ViewType.CostumeChangeInfo], container.transform);
                CostumeChangeIndicator costumeChangeIndicator = result.GetComponent<CostumeChangeIndicator>();
                if (costumeChangeIndicator != null)
                {
                    ApplianceGridView upgradeIndicator = result.AddComponent<ApplianceGridView>();
                    upgradeIndicator.container = (Transform)f_Container?.GetValue(costumeChangeIndicator);
                    upgradeIndicator.rootMenuConfig = new ApplianceGridConfig();
                    Component.DestroyImmediate(costumeChangeIndicator);
                }
            }

            __result = result;
            return false;
        }
    }
}