using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class MaxHealthBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.currentUnit.Health == context.constants.UnitHealth ? 1 : 0;
        }
    }
}