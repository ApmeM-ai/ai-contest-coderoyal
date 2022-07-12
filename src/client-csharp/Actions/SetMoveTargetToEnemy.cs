using System.Linq;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class SetMoveTargetToEnemy : IAction<AIState>
    {
        public void Execute(AIState context)
        {
            var closestEnemy = context.communicationState.EnemyMemory
                .Select(a => a.Item)
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                .Cast<Unit>()
                .First();

            context.unitState.MoveToPoint = closestEnemy.Position;
        }
    }
}