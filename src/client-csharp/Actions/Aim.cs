using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class Aim : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.result = new UnitOrder(
                    context.result?.TargetVelocity ?? new Vec2(0, 0),
                    context.result?.TargetDirection ?? new Vec2(0, 0),
                    new ActionOrder.Aim(true));
            }
        }
 }