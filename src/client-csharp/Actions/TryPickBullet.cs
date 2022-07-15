using System;
using System.Linq;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;
using static AiCup22.Model.Item;

namespace AiCup22
{
    public class PickupLoot : IAction<AIState>
    {
        public void Execute(AIState context)
        {
            var closestAmmo = context.communicationState.LootMemory
                .Select(a => a.Item)
                .Where(a => IsRequired(context, a.Item))
                .Where(a => a.Position.WithinZone(context.game))
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                .Cast<Loot?>()
                .FirstOrDefault();

            if (closestAmmo == null)
            {
                return;
            }

            if (closestAmmo.Value.Position.Sub(context.currentUnit.Position).GetLengthQuad() < context.constants.UnitRadius * context.constants.UnitRadius)
            {
                context.result = new UnitOrder(
                    context.result?.TargetVelocity ?? new Vec2(0, 0),
                    context.result?.TargetDirection ?? new Vec2(0, 0),
                    new ActionOrder.Pickup(closestAmmo.Value.Id));
            }
        }

        private bool IsRequired(AIState context, Item item)
        {
            return 
                (item is ShieldPotions && context.currentUnit.ShieldPotions < context.constants.MaxShieldPotionsInInventory) ||
                (item is Ammo && context.currentUnit.Ammo[((Ammo)item).WeaponTypeIndex] < context.constants.Weapons[((Ammo)item).WeaponTypeIndex].MaxInventoryAmmo);
        }
    }
}