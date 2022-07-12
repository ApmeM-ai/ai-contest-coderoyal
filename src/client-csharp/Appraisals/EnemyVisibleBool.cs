using System.Linq;
using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class EnemyVisibleBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            return context.game.Units
                .Where(a => a.PlayerId != context.game.MyId)
                .Any() ? 1 : 0;
        }
    }
}