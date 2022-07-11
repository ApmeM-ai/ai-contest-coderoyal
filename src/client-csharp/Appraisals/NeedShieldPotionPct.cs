using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class NeedShieldPotionPct : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                var isMaxShield = (context.currentUnit.Shield == context.constants.MaxShield);
                var pct = context.currentUnit.ShieldPotions / context.constants.MaxShieldPotionsInInventory;
                return (float)((1 - pct) * (isMaxShield ? 0.75 : 1));
            }
        }
 }