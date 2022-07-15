using System.Linq;
using AiCup22.Model;
using AiCup22.UtilityAI.Appraisals;
using static AiCup22.Model.Item;

namespace AiCup22
{
    public class ShouldPickupLootBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            var closestAmmo = context.communicationState.LootMemory
                .Select(a => a.Item)
                .Where(a => IsRequired(context, a.Item))
                .Where(a => a.Position.WithinZone(context.game))
                .Where(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad() < context.constants.UnitRadius * context.constants.UnitRadius)
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                .Cast<Loot?>()
                .FirstOrDefault();

            if (closestAmmo == null)
            {
                return 0;
            }

            return 1;
        }

        private bool IsRequired(AIState context, Item item)
        {
            return 
                (item is ShieldPotions && context.currentUnit.ShieldPotions < context.constants.MaxShieldPotionsInInventory) ||
                (item is Ammo && context.currentUnit.Ammo[((Ammo)item).WeaponTypeIndex] < context.constants.Weapons[((Ammo)item).WeaponTypeIndex].MaxInventoryAmmo);
        }
    }
}