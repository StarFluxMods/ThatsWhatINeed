using Kitchen;
using KitchenMods;
using ThatsWhatINeed.Components;
using Unity.Entities;

namespace ThatsWhatINeed.Systems
{
    [UpdateBefore(typeof(ShowPingedApplianceInfo))]
    public class ActivateGridDuringNight : ApplianceInteractionSystem, IModSystem
    {
        protected override InteractionType RequiredType => InteractionType.Notify;

        protected override bool IsPossible(ref InteractionData data)
        {
            return Has<CGrantsExtraBlueprint>(data.Target);
        }

        protected override void Perform(ref InteractionData data)
        {
            EntityManager.AddComponentData(data.Target, new CTriggerApplianceGrid
            {
                IsTriggered = true,
                TriggerEntity = data.Interactor
            });
        }
    }
}
