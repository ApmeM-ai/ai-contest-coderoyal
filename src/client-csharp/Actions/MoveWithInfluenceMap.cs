using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;
using BrainAI.InfluenceMap.Fading;
using BrainAI.InfluenceMap.VectorGenerator;
using BrainAI.Pathfinding;

namespace AiCup22
{
    public class MoveWithInfluenceMap : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.communicationState.map.AddCharge(
                    "target",
                    new PointChargeOrigin(new Point((int)context.unitState.MoveToPoint.X, (int)(context.unitState.MoveToPoint.Y))),
                    new ConstantInRadiusFading(float.MaxValue),
                    10);

                var forceDrection = context.communicationState.map.FindForceDirection(new Point((int)context.currentUnit.Position.X, (int)context.currentUnit.Position.Y));
                context.communicationState.map.ClearLayer("target");
                var move = new Vec2(forceDrection.X, forceDrection.Y);

                context.result = new UnitOrder(move, context.result?.TargetDirection ?? new Vec2(0, 0), context.result?.Action);

                context.debug?.AddPolyLine(
                    new Vec2[] { context.currentUnit.Position, context.currentUnit.Position.Add(move) },
                    0.5,
                    new Debugging.Color(0, 1, 0, 1));
            }
        }
 }