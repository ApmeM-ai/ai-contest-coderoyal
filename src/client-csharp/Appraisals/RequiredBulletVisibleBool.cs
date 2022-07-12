using System.Linq;
using AiCup22.UtilityAI.Appraisals;
using static AiCup22.Model.Item;

namespace AiCup22
{
    public class RequiredBulletVisibleBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.communicationState.LootMemory
                .Select(a => a.Item)
                .Where(a => a.Item is Ammo)
                .Where(a => ((Ammo)a.Item).WeaponTypeIndex == context.currentUnit.Weapon.Value)
                .Any() ? 1 : 0;
        }
    }
}