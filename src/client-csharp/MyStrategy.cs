using System.Collections.Generic;
using AiCup22.Model;

namespace AiCup22
{
    public class MyStrategy
    {
        private readonly Constants constants;

        public MyStrategy(Constants constants) {
            this.constants = constants;
        }

        public Order GetOrder(Game game, DebugInterface debugInterface)
        {
            var orders = new Dictionary<int, UnitOrder>();
            foreach (var unit in game.Units)
            {
                if (unit.PlayerId != game.MyId)
                {
                    continue;
                }
                orders[unit.Id] = new UnitOrder(
                    new Vec2(-unit.Position.X, -unit.Position.Y),
                    new Vec2(-unit.Direction.Y, unit.Direction.X),
                    new ActionOrder.Aim(true));
            }

            return new Order(orders);
        }
 
        public void DebugUpdate(DebugInterface debugInterface) { 
        }
 
        public void Finish() { 
        }
    }
}