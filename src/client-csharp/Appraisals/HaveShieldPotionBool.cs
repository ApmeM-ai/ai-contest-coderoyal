using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class HaveShieldPotionBool : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                if (context.currentUnit.ShieldPotions == 0)
                {
                    return 0;
                }

                return 1;
            }
        }
 }