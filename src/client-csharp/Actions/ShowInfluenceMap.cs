using System;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;
using BrainAI.Pathfinding;

namespace AiCup22
{
    public class ShowInfluenceMap : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                for (var x = -(int)context.constants.ViewDistance / 4; x < (int)context.constants.ViewDistance / 4; x++)
                    for (var y = -(int)context.constants.ViewDistance / 4; y < (int)context.constants.ViewDistance / 4; y++)
                    {
                        var pos = new Point((int)context.currentUnit.Position.X + x, (int)context.currentUnit.Position.Y + y);
                        var charge = context.communicationState.map.FindForceDirection(pos);
                        var chargeLength = Math.Sqrt(charge.X * charge.X + charge.Y * charge.Y);
                        if (chargeLength > 1)
                        {
                            context.debug?.AddPolyLine(
                            new Vec2[] {
                                new Vec2(context.currentUnit.Position.X + x, context.currentUnit.Position.Y + y),
                                new Vec2(context.currentUnit.Position.X + x + charge.X / chargeLength, context.currentUnit.Position.Y + y + charge.Y / chargeLength),
                                },
                            0.1,
                            new Debugging.Color(1, 0, 0, 1));
                        }
                    }
            }
        }
 }