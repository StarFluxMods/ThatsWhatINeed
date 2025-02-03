using Kitchen;
using Kitchen.ShopBuilder;
using KitchenMods;
using ThatsWhatINeed.Components;
using Unity.Collections;
using Unity.Entities;

namespace ThatsWhatINeed.Systems
{
    public class CleanOldIndicators : GameSystemBase, IModSystem
    {
        /*
         * This system is a band-aid fix to cleanup UI indicators without assigned players.
         */
        
        private EntityQuery _indicators;
        private EntityQuery _players;
        protected override void Initialise()
        {
            base.Initialise();
            _indicators = GetEntityQuery(new QueryHelper().All(typeof(CApplianceGridInfo)));
            _players = GetEntityQuery(new QueryHelper().All(typeof(CPlayer)));
        }

        protected override void OnUpdate()
        {
            using (NativeArray<Entity> indicators = _indicators.ToEntityArray(Allocator.Temp))
            {
                using (NativeArray<Entity> players = _players.ToEntityArray(Allocator.Temp))
                {
                    foreach (Entity indicator in indicators)
                    {
                        if (Require(indicator, out CApplianceGridInfo cApplianceGridInfo))
                        {
                            bool foundPlayer = false;
                            foreach (Entity player in players)
                            {
                                if (Require(player, out CPlayer cPlayer))
                                {
                                    if (cPlayer.ID == cApplianceGridInfo.Player.PlayerID)
                                    {
                                        foundPlayer = true;
                                        break;
                                    }
                                }
                            }

                            if (!foundPlayer)
                            {
                                EntityManager.DestroyEntity(indicator);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}