using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class SetLookScan : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.result = new UnitOrder(
                    context.result?.TargetVelocity ?? new Vec2(0, 0),
                    new Vec2(-context.currentUnit.Direction.Y, context.currentUnit.Direction.X),
                    context.result?.Action);
            }
        }
 }