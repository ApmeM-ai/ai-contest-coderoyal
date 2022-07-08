using System.Collections.Generic;
using System.Linq;
using AiCup22.Model;

namespace AiCup22
{
    public class MyStrategy
    {
        private readonly Constants constants;

        public MyStrategy(Constants constants)
        {
            this.constants = constants;
        }

        public Order GetOrder(Game game, DebugInterface debugInterface)
        {
            var actionAim = new ActionOrder.Aim(true);
            if (actionAim == null)
            {
                actionAim = new ActionOrder.Aim(false);
            }

            var result = new Dictionary<int, AiCup22.Model.UnitOrder>();

            var myUnits = game.Units.Where(a => a.PlayerId == game.MyId);

            foreach (var myUnit in myUnits)
            {
                var closestEnemy = game.Units
                    .Where(a => a.PlayerId != game.MyId)
                    .OrderBy(a => a.Position.Sub(myUnit.Position).GetLengthQuad())
                    .Take(1)
                    .Cast<Unit?>()
                    .FirstOrDefault();
                
                if (closestEnemy == null) {
                    var targetVelocity = game.Zone.CurrentCenter.Sub(myUnit.Position);
                    var targetDirection = game.Zone.CurrentCenter.Sub(myUnit.Position);
                    var UnitOrder1 = new UnitOrder(targetVelocity, targetDirection, actionAim);
                    result.Add(myUnit.Id, UnitOrder1);
                } else {
                    var targetVelocity = closestEnemy.Value.Position.Sub(myUnit.Position);
                    var targetDirection = closestEnemy.Value.Position.Sub(myUnit.Position);

                    var UnitOrder1 = new UnitOrder(targetVelocity, targetDirection, actionAim);
                    result.Add(myUnit.Id, UnitOrder1);
                }
            }

            return new Order(result);
        }

        public void DebugUpdate(DebugInterface debugInterface)
        {
        }

        public void Finish()
        {
        }
    }

    public static class Extensions
    {
        public static double GetLengthQuad(this Vec2 v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        public static Vec2 Add(this Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X + v2.X, v1.Y + v2.Y);
        }
        public static Vec2 Sub(this Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X - v2.X, v1.Y - v2.Y);
        }


    }
}