using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class HaveBulletsBool : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                if (context.currentUnit.Ammo[context.currentUnit.Weapon.Value] == 0)
                {
                    return 0;
                }

                return 1;
            }
        }
 }