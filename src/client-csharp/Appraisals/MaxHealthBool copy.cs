using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class MaxShieldBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.currentUnit.Shield == context.constants.MaxShield ? 1 : 0;
        }
    }
}