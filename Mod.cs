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
using Shapes;
using ThatsWhatINeed.Menus;
using ThatsWhatINeed.Views;
using UnityEngine;
using KitchenLogger = KitchenLib.Logging.KitchenLogger;

namespace ThatsWhatINeed
{
    public class Mod : BaseMod, IModSystem
    {
        public const string MOD_GUID = "com.starfluxgames.thatswhatineed";
        public const string MOD_NAME = "Thats What I Need";
        public const string MOD_VERSION = "0.1.3";
        public const string MOD_AUTHOR = "StarFluxGames";
        public const string MOD_GAMEVERSION = ">=1.2.0";

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

        private void SetupWarningIndicatorPrefab()
        {
            GameObject indicator = Bundle.LoadAsset<GameObject>("WarningIndicator");
            
            Rectangle Back = indicator.GetChild("Container/GameObject/Container/GameObject (1)/Patience Container/Back").AddComponent<Rectangle>();
            Rectangle Gray = indicator.GetChild("Container/GameObject/Container/GameObject (1)/Patience Container/Gray").AddComponent<Rectangle>();
            Rectangle Bar = indicator.GetChild("Container/GameObject/Container/GameObject (1)/Patience Container/Bar").AddComponent<Rectangle>();
                
            Back.Color = new Color(0.06441849f, 0.0489854f, 0.1509434f, 0.4980392f);
            Gray.Color = new Color(0.2784314f, 0.2784314f, 0.3529412f);
            Bar.Color = new Color(0.5882353f, 0.9882354f, 0.1843137f);

            Back.Width = 0.4f;
            Back.Height = 0.7f;
            Gray.Width = 0.3f;
            Gray.Height = 0.6f;
            Bar.Width = 0.3f;
            Bar.Height = 0.6f;

            Back.Type = Rectangle.RectangleType.RoundedSolid;
            Gray.Type = Rectangle.RectangleType.RoundedSolid;
            Bar.Type = Rectangle.RectangleType.RoundedSolid;
                
            Back.CornerRadiii = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
            Gray.CornerRadiii = new Vector4(0.02f, 0.02f, 0.02f, 0.02f);
            Bar.CornerRadiii = new Vector4(0.02f, 0.02f, 0.02f, 0.02f);
                
            Back.Thickness = 0.02f;
            Gray.Thickness = 0.02f;
            Bar.Thickness = 0.02f;
            
            WarningIndicatorView view = indicator.AddComponent<WarningIndicatorView>();

            view.Container = indicator.GetChild("Container");
            view.Animator = indicator.GetChild("Container/GameObject/Container").GetComponent<Animator>();
            view.Patience = Bar;
            view.bellSound = indicator.GetChild("BellSound").GetComponent<AudioSource>();
            view.scoomSound = indicator.GetChild("ScoomSound").GetComponent<AudioSource>();
            
            view.Container.SetActive(false);
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).FirstOrDefault() ?? throw new MissingAssetBundleException(MOD_GUID);
            Logger = InitLogger();

            SetupWarningIndicatorPrefab();
            
            Manager = new PreferenceManager(MOD_GUID);

            Manager.RegisterPreference(new PreferenceInt("notificationSound", 1));
            Manager.RegisterPreference(new PreferenceFloat("bellVolume", 0.25f));
            Manager.RegisterPreference(new PreferenceFloat("scoomVolume", 0.25f));
            Manager.RegisterPreference(new PreferenceBool("shouldWarningFlash", true));
            Manager.RegisterPreference(new PreferenceFloat("warningPercentage", 0.5f));
            Manager.Load();
            Manager.Save();

            Events.BuildGameDataEvent += (s, args) =>
            {
                if (!args.firstBuild) return;

                Appliance desk = args.gamedata.Get<Appliance>(ApplianceReferences.BlueprintOrderingDesk);
                
                GameObject warningIndicator = Bundle.LoadAsset<GameObject>("WarningIndicator");
                warningIndicator.transform.parent = desk.Prefab.GetChild("Container").transform;
                warningIndicator.transform.localPosition = new Vector3(-0.5f, -0.8f, 0.2f);
            };
            
            ModsPreferencesMenu<MenuAction>.RegisterMenu(MOD_NAME, typeof(PreferenceMenu<MenuAction>), typeof(MenuAction));
            ModsPreferencesMenu<MenuAction>.RegisterMenu(MOD_NAME, typeof(PreferenceMenu<MenuAction>), typeof(MenuAction));
            
            Events.MainMenuView_SetupMenusEvent += (s, args) =>
            {
                args.addMenu.Invoke(args.instance, new object[] { typeof(PreferenceMenu<MenuAction>), new PreferenceMenu<MenuAction>(args.instance.ButtonContainer, args.module_list) });
            };
            
            Events.PlayerPauseView_SetupMenusEvent += (s, args) =>
            {
                args.addMenu.Invoke(args.instance, new object[] { typeof(PreferenceMenu<MenuAction>), new PreferenceMenu<MenuAction>(args.instance.ButtonContainer, args.module_list) });
            };
        }
    }
}

