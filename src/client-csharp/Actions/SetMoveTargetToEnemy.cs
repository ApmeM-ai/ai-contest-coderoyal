using System.Linq;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class SetMoveTargetToEnemy : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                var closestEnemy = context.game.Units
                    .Where(a => a.PlayerId != context.game.MyId)
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .Cast<Unit>()
                    .First();

                context.unitState.MoveToPoint = closestEnemy.Position;
            }
        }
 }