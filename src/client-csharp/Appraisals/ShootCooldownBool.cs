using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class ShootCooldownBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.currentUnit.NextShotTick > context.game.CurrentTick ? 1 : 0;
        }
    }
}