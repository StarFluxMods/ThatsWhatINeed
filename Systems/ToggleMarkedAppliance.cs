using Kitchen;
using KitchenMods;
using ThatsWhatINeed.Components;
using Unity.Entities;

namespace ThatsWhatINeed.Systems
{
    public class ToggleMarkedAppliance : GameSystemBase, IModSystem
    {
        public static ToggleMarkedAppliance instance;
        
        protected override void OnUpdate()
        {
            instance ??= this;
        }

        public void ToggleApplianceID(int applianceID)
        {
            if (TryGetSingletonEntity<SMarkedBlueprint>(out Entity entity))
            {
                DynamicBuffer<SMarkedBlueprint> buffer = GetBuffer<SMarkedBlueprint>(entity);
                bool found = false;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == applianceID)
                    {
                        buffer.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    buffer.Add(applianceID);
                }
            }
        }
    }
}