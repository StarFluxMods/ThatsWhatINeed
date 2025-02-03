using Kitchen;
using KitchenLib.Utils;
using KitchenMods;
using ThatsWhatINeed.Components;
using Unity.Entities;

namespace ThatsWhatINeed.Systems
{
    public class ManageApplianceGridIndicators : IndicatorManager, IModSystem
    {
        protected override ViewType ViewType => (ViewType)VariousUtils.GetID("com.starfluxgames.thatswhatineed.appliancegridview");

        protected override EntityQuery GetSearchQuery()
        {
            return GetEntityQuery(typeof(CGrantsExtraBlueprint), typeof(CPosition));
        }

        protected override bool ShouldHaveIndicator(Entity candidate)
        {
            if (Require(candidate, out CHasIndicator comp))
                return Require(comp.Indicator, out CApplianceGridInfo cApplianceGridInfo) && !cApplianceGridInfo.IsComplete;
            
            if (!Has<CPosition>(candidate)) return false;
            if (!Require(candidate, out CTriggerApplianceGrid trigger)) return false;
            if (!Has<CPlayer>(trigger.TriggerEntity)) return false;
            if (!trigger.IsTriggered) return false;
            
            trigger.IsTriggered = false;
            Set(candidate, trigger);
            return true;

        }

        protected override Entity CreateIndicator(Entity source)
        {
            if (!Require(source, out CPosition position)) return default(Entity);
            if (!Require(source, out CGrantsExtraBlueprint cPet)) return default(Entity);
            if (!Require(source, out CTriggerApplianceGrid trigger)) return default(Entity);
            if (!Require<CPlayer>(trigger.TriggerEntity, out CPlayer player)) return default(Entity);
            
            Entity entity = base.CreateIndicator(source);
            EntityManager.AddComponentData(entity, new CPosition(position));
            EntityManager.AddComponentData(entity, new CApplianceGridInfo
            {
                Player = player
            });
            EntityManager.AddComponentData(entity, new CDoNotPersist());
            return entity;
        }
    }
}
