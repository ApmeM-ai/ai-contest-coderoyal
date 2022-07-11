using System;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;

namespace AiCup22
{
    public class SetMoveTargetToRandomPoint : IAction<AIState>
        {
            private static Random r = new Random();

            public void Execute(AIState context)
            {
                if (!context.unitState.RandomPoint.WithinZone(context.game))
                {
                    context.unitState.RandomPoint = context.currentUnit.Position;
                }

                if (context.unitState.RandomPoint.Sub(context.currentUnit.Position).GetLengthQuad() < 16)
                {
                    var randomDistance = (r.NextDouble() * 2 - 1) * context.game.Zone.CurrentRadius;
                    var randomAngle = r.NextDouble() * Math.PI * 2;

                    var x = Math.Cos(randomAngle) * randomDistance + context.game.Zone.CurrentCenter.X;
                    var y = Math.Sin(randomAngle) * randomDistance + context.game.Zone.CurrentCenter.Y;

                    context.unitState.RandomPoint = new Vec2(x, y);
                }

                context.unitState.MoveToPoint = context.unitState.RandomPoint;
            }
        }
 }