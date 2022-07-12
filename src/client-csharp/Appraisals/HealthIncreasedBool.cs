using System.Linq;
using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class HealthIncreasedLastTickBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.unitState.PreviousUnit.Health < context.currentUnit.Health ? 1 : 0;
        }
    }
}