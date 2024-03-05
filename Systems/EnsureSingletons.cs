using Kitchen;
using KitchenMods;
using ThatsWhatINeed.Components;

namespace ThatsWhatINeed.Systems
{
    public class EnsureSingletons : GameSystemBase, IModSystem
    {
        protected override void OnUpdate()
        {
            if (!HasSingleton<SAvailableBlueprint>())
            {
                EntityManager.CreateEntity(typeof(SAvailableBlueprint), typeof(CDoNotPersist));
            }
            if (!HasSingleton<SMarkedBlueprint>())
            {
                EntityManager.CreateEntity(typeof(SMarkedBlueprint), typeof(CDoNotPersist));
            }
        }
    }
}