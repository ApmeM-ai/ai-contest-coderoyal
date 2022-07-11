using System.Linq;
using AiCup22.Model;
using BrainAI.AI.UtilityAI.Actions;
using static AiCup22.Model.Item;

namespace AiCup22
{
    public class TryPickShield : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                var closestAmmo = context.game.Loot
                    .Where(a => a.Item is ShieldPotions)
                    .Where(a => a.Position.WithinZone(context.game))
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .Cast<Loot?>()
                    .FirstOrDefault();

                if (closestAmmo == null){
                    return;
                }

                if (closestAmmo.Value.Position.Sub(context.currentUnit.Position).GetLengthQuad() < context.constants.UnitRadius * context.constants.UnitRadius)
                {
                    context.result = new UnitOrder(
                        context.result?.TargetVelocity ?? new Vec2(0, 0),
                        context.result?.TargetDirection ?? new Vec2(0, 0),
                        new ActionOrder.Pickup(closestAmmo.Value.Id));
                }
            }
        }
 }