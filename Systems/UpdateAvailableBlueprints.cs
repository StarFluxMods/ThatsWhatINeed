using Kitchen;
using Kitchen.ShopBuilder;
using KitchenMods;
using ThatsWhatINeed.Components;
using Unity.Collections;
using Unity.Entities;

namespace ThatsWhatINeed.Systems
{
    public class UpdateAvailableBlueprints : GameSystemBase, IModSystem
    {
        private EntityQuery _shopBuilderOptions;
        
        protected override void Initialise()
        {
            base.Initialise();
            _shopBuilderOptions = GetEntityQuery(new QueryHelper().All(typeof(CShopBuilderOption)));
        }

        protected override void OnUpdate()
        {
            if (TryGetSingletonEntity<SAvailableBlueprint>(out Entity entity))
            {
                using NativeArray<CShopBuilderOption> options = _shopBuilderOptions.ToComponentDataArray<CShopBuilderOption>(Allocator.Temp);

                DynamicBuffer<SAvailableBlueprint> buffer = GetBuffer<SAvailableBlueprint>(entity);
                buffer.Clear();
                
                for (int i = 0; i < options.Length; i++)
                {
                    CShopBuilderOption option = options[i];
                    if (option.IsSuitableForDesk())
                        buffer.Add(options[i].Appliance);
                }
            }
        }
    }
}