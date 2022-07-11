using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class HealthPct : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                return (float)(context.currentUnit.Health / context.constants.UnitHealth);
            }
        }
 }