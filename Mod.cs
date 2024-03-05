using KitchenLib;
using KitchenLib.Logging;
using KitchenLib.Logging.Exceptions;
using KitchenMods;
using System.Linq;
using System.Reflection;
using Kitchen;
using KitchenData;
using KitchenLib.Event;
using KitchenLib.Preferences;
using KitchenLib.References;
using KitchenLib.Utils;
using ThatsWhatINeed.Menus;
using UnityEngine;

namespace ThatsWhatINeed
{
    public class Mod : BaseMod, IModSystem
    {
        public const string MOD_GUID = "com.starfluxgames.thatswhatineed";
        public const string MOD_NAME = "Thats What I Need";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "StarFluxGames";
        public const string MOD_GAMEVERSION = ">=1.1.9";

        public static AssetBundle Bundle;
        public static KitchenLogger Logger;
        public static PreferenceManager Manager;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            Logger.LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).FirstOrDefault() ?? throw new MissingAssetBundleException(MOD_GUID);
            Logger = InitLogger();

            Manager = new PreferenceManager(MOD_GUID);

            Manager.RegisterPreference(new PreferenceInt("notificationSound", 1));
            Manager.RegisterPreference(new PreferenceFloat("bellVolume", 0.25f));
            Manager.RegisterPreference(new PreferenceFloat("scoomVolume", 0.25f));
            Manager.Load();
            Manager.Save();

            Events.BuildGameDataEvent += (s, args) =>
            {
                if (!args.firstBuild) return;

                Appliance desk = args.gamedata.Get<Appliance>(ApplianceReferences.BlueprintOrderingDesk);
                GameObject icon = Bundle.LoadAsset<GameObject>("Icon");
                ThatsWhatINeed.Views.BlueprintDeskView view = icon.AddComponent<ThatsWhatINeed.Views.BlueprintDeskView>();

                icon.transform.parent = desk.Prefab.GetChild("Container").transform;
                icon.transform.localPosition = new Vector3(0, 0, 0);

                view.prefab = icon.GetChild("Container");
                view.bellSound = icon.GetChild("BellSound").GetComponent<AudioSource>();
                view.scoomSound = icon.GetChild("ScoomSound").GetComponent<AudioSource>();
            };
            
            ModsPreferencesMenu<MainMenuAction>.RegisterMenu(MOD_NAME, typeof(PreferenceMenu<MainMenuAction>), typeof(MainMenuAction));
            ModsPreferencesMenu<PauseMenuAction>.RegisterMenu(MOD_NAME, typeof(PreferenceMenu<PauseMenuAction>), typeof(PauseMenuAction));
            
            Events.MainMenuView_SetupMenusEvent += (s, args) =>
            {
                args.addMenu.Invoke(args.instance, new object[] { typeof(PreferenceMenu<MainMenuAction>), new PreferenceMenu<MainMenuAction>(args.instance.ButtonContainer, args.module_list) });
            };
            
            Events.PlayerPauseView_SetupMenusEvent += (s, args) =>
            {
                args.addMenu.Invoke(args.instance, new object[] { typeof(PreferenceMenu<PauseMenuAction>), new PreferenceMenu<PauseMenuAction>(args.instance.ButtonContainer, args.module_list) });
            };
        }
    }
}

