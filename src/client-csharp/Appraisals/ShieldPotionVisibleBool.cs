using System.Linq;
using AiCup22.UtilityAI.Appraisals;
using static AiCup22.Model.Item;

namespace AiCup22
{
    public class ShieldPotionVisibleBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.communicationState.LootMemory
                .Select(a => a.Item)
                .Where(a => a.Item is ShieldPotions)
                .Where(a => a.Position.WithinZone(context.game))
                .Any() ? 1 : 0;
        }
    }
}