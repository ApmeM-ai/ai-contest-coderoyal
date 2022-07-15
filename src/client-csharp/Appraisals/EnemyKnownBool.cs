using System.Linq;
using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class EnemyKnownBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.communicationState.EnemyMemory.Any() ? 1 : 0;
        }
    }
}