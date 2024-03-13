using System;
using Kitchen;
using KitchenLib.Preferences;
using KitchenMods;
using MessagePack;
using Shapes;
using ThatsWhatINeed.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ThatsWhatINeed.Views
{
    public class WarningIndicatorView : UpdatableObjectView<WarningIndicatorView.ViewData>
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
                    if (Require(entities[i], out CGrantsExtraBlueprint cGrantsExtraBlueprint) && Require(entities[i], out CApplianceDeskIterate cApplianceDeskIterate))
                    {
                        ViewData data = new ViewData();
                        
                        if (TryGetSingletonEntity<STime>(out Entity sTimeEntity) && Require(sTimeEntity, out STime sTime))
                        {
                            data.timeRemaining = cApplianceDeskIterate.NextUpdateTime - sTime.TimeOfDayUnbounded;
                        }
                        
                        foreach (SMarkedBlueprint markedBlueprint in buffer)
                        {
                            if (markedBlueprint != cGrantsExtraBlueprint.ID || cGrantsExtraBlueprint.ID == 0) continue;
                            data.shouldShow = !cApplianceDeskIterate.IsLocked;
                            break;
                        }

                        SendUpdate(view, data);
                    }
                }
            }
        }
        
        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<WarningIndicatorView>();

            [Key(0)] public bool shouldShow;
            [Key(1)] public float timeRemaining;
            
            public bool IsChangedFrom(ViewData cached)
            {
                return shouldShow != cached.shouldShow || timeRemaining != cached.timeRemaining;
            }
        }

        public GameObject Container;
        public Animator Animator;
        public Rectangle Patience;
        public AudioSource bellSound;
        public AudioSource scoomSound;
        
        private float BarFullY;

        private float BarFullHeight;
        
        private static readonly int IsWarning = Animator.StringToHash("isWarning");

        private void Awake()
        {
            BarFullY = Patience.transform.localPosition.y;
            BarFullHeight = Patience.Height;
        }

        protected override void UpdateData(ViewData data)
        {
            Container.SetActive(data.shouldShow);
            Animator.SetBool(IsWarning, (data.timeRemaining * 10) < Mod.Manager.GetPreference<PreferenceFloat>("warningPercentage").Value);
            SetBar(data.timeRemaining * 10);
            
            if (data.shouldShow && Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Value != 0)
            {
                if (Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Value == 1 && !bellSound.isPlaying) // Bell Sound
                {
                    bellSound.volume = Mod.Manager.GetPreference<PreferenceFloat>("bellVolume").Value;
                    bellSound.Play();
                }
                else if (Mod.Manager.GetPreference<PreferenceInt>("notificationSound").Value == 2 && !scoomSound.isPlaying) // Scoom Sound
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

        private void Update()
        {
            
        }

        private void SetBar(float fraction)
        {
            if (fraction > 1f) fraction = 1f;
            float num = fraction * BarFullHeight;
            Vector3 localPosition = Patience.transform.localPosition;
            localPosition.y = BarFullY - (BarFullHeight - num) / 2f;
            Patience.transform.localPosition = localPosition;
            Patience.Height = num;
            
            float num2 = 1f - num;
            float num3 = num2 * num2 * num2;
            float f = 50f * (1f - fraction / 0.5f);
            
            Color warn = Color.Lerp(new Color(0.27f, 0.27f, 0.35f), new Color(0.93f, 0.27f, 0.12f) * Mathf.Pow(2f, 0.5f), Mathf.Abs(Mathf.Sin(f)) * num3);

            if (fraction < Mod.Manager.GetPreference<PreferenceFloat>("warningPercentage").Value)
            {
                if (Mod.Manager.GetPreference<PreferenceBool>("shouldWarningFlash").Value)
                {
                    Patience.Color = warn;
                }
                else
                {
                    Patience.Color = new Color(0.93f, 0.27f, 0.12f);
                }

            }
            else 
            {
                Patience.Color = new Color(0.5882353f, 0.9882354f, 0.1843137f);
            }
        }
    }
}