using Kitchen;
using KitchenLib.Preferences;
using KitchenMods;
using MessagePack;
using ThatsWhatINeed.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ThatsWhatINeed.Views
{
    public class BlueprintDeskView : UpdatableObjectView<BlueprintDeskView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;
            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(new QueryHelper().All(typeof(CGrantsExtraBlueprint), typeof(CApplianceDeskIterate), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                if (!TryGetSingletonEntity<SMarkedBlueprint>(out Entity entity)) return;
                
                DynamicBuffer<SMarkedBlueprint> buffer = GetBuffer<SMarkedBlueprint>(entity);
                
                using NativeArray<Entity> entities = Views.ToEntityArray(Allocator.Temp);
                using NativeArray<CLinkedView> views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                for (int i = 0; i < views.Length; i++)
                {
                    CLinkedView view = views[i];
                    if (Require(entities[i], out CGrantsExtraBlueprint cGrantsExtraBlueprint))
                    {
                        ViewData data = new ViewData
                        {
                            shouldWarn = false
                        };
                        foreach (SMarkedBlueprint markedBlueprint in buffer)
                        {
                            if (markedBlueprint == cGrantsExtraBlueprint.ID && cGrantsExtraBlueprint.ID != 0)
                            {
                                data.shouldWarn = true; 
                                data.selectedAppliance = cGrantsExtraBlueprint.ID;
                                break;
                            }
                        }

                        SendUpdate(view, data);
                    }
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool shouldWarn;
            [Key(1)] public int selectedAppliance;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<BlueprintDeskView>();

            public bool IsChangedFrom(ViewData cached)
            {
                return shouldWarn != cached.shouldWarn || selectedAppliance != cached.selectedAppliance;
            }
        }

        public GameObject prefab;
        public AudioSource bellSound;
        public AudioSource scoomSound;
        
        protected override void UpdateData(ViewData data)
        {
            // Maybe add some audio here
            prefab.SetActive(data.shouldWarn);
            
            if (data.shouldWarn && Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Value != 0)
            {
                if (Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Value == 1) // Bell Sound
                {
                    bellSound.volume = Mod.Manager.GetPreference<PreferenceFloat>("bellVolume").Value;
                    bellSound.Play();
                }
                else if (Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Value == 2) // Scoom Sound
                {
                    scoomSound.volume = Mod.Manager.GetPreference<PreferenceFloat>("scoomVolume").Value;
                    scoomSound.Play();
                }
            }
            else
            {
                bellSound.Stop();
                scoomSound.Stop();
            }

        }
    }
}