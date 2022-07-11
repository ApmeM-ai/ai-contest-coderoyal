using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class ShieldPct : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                return (float)(context.currentUnit.Shield / context.constants.MaxShield);
            }
        }
 }