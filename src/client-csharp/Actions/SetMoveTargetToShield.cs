using System.Linq;
using BrainAI.AI.UtilityAI.Actions;
using static AiCup22.Model.Item;

namespace AiCup22
{
    public class SetMoveTargetToShield : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                var closestPotion = context.game.Loot
                    .Where(a => a.Item is ShieldPotions)
                    .Where(a => a.Position.WithinZone(context.game))
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .First();

                context.debug?.AddCircle(closestPotion.Position, 1, new Debugging.Color(0, 1, 1, 0.5));

                context.unitState.MoveToPoint = closestPotion.Position;
            }
        }
 }