using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class MaxAimBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.currentUnit.Aim == 1 ? 1 : 0;
        }
    }
}