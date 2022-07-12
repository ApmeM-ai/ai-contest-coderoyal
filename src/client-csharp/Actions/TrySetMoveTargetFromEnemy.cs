using System.Linq;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class TrySetMoveTargetFromEnemy : IAction<AIState>
    {
        public void Execute(AIState context)
        {
            var closestEnemy = context.communicationState.EnemyMemory
                .Select(a => a.Item)
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                .Cast<Unit>()
                .First();

            var newPoint = context.currentUnit.Position.Sub(closestEnemy.Position).Add(context.currentUnit.Position);
            if (newPoint.WithinZone(context.game))
            {
                context.unitState.MoveToPoint = newPoint;
            }
        }
    }
}