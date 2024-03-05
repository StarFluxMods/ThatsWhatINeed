using KitchenMods;
using Unity.Entities;

namespace ThatsWhatINeed.Components
{
    public struct CTriggerApplianceGrid : IModComponent
    {
        public bool IsTriggered;
        public Entity TriggerEntity;
    }
}