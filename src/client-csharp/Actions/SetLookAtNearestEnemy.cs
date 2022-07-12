using System.Linq;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class SetLookAtNearestEnemy : IAction<AIState>
    {
        public void Execute(AIState context)
        {
            var closestEnemy = context.communicationState.EnemyMemory
                .Select(a => a.Item)
                .Where(a => a.PlayerId != context.game.MyId)
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                .Cast<Unit?>()
                .FirstOrDefault();

            if (closestEnemy == null)
            {
                return;
            }

            context.result = new UnitOrder(
                context.result?.TargetVelocity ?? new Vec2(0, 0),
                closestEnemy.Value.Position.Sub(context.currentUnit.Position),
                context.result?.Action);
        }
    }
}