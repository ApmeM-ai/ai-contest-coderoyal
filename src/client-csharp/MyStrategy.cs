using System.Collections.Generic;
using System.Linq;
using AiCup22.Model;
using BrainAI.AI.UtilityAI;
using BrainAI.AI.UtilityAI.Actions;
using BrainAI.AI.UtilityAI.Considerations;
using BrainAI.AI.UtilityAI.Considerations.Appraisals;
using BrainAI.AI.UtilityAI.Reasoners;

namespace AiCup22
{
    public class MyStrategy
    {
        private readonly Constants constants;

        public MyStrategy(Constants constants)
        {
            this.constants = constants;
        }

        Dictionary<int, UtilityAI<AIState>> units = new Dictionary<int, UtilityAI<AIState>>();

        public Order GetOrder(Game game, DebugInterface debugInterface)
        {
            var result = new Dictionary<int, AiCup22.Model.UnitOrder>();

            var myUnits = game.Units.Where(a => a.PlayerId == game.MyId);

            foreach (var myUnit in myUnits)
            {
                if (!units.ContainsKey(myUnit.Id))
                {
                    units[myUnit.Id] = BuildAI(new AIState());
                }

                units[myUnit.Id].context.game = game;
                units[myUnit.Id].context.currentUnit = myUnit;
                units[myUnit.Id].context.result = null;
                units[myUnit.Id].Tick();
                result.Add(myUnit.Id, units[myUnit.Id].context.result.Value);
            }

            return new Order(result);
        }

        public void DebugUpdate(DebugInterface debugInterface)
        {
            if (debugInterface == null)
            {
                return;
            }

            debugInterface.Clear();
            foreach (var unit in units.Values)
            {
                debugInterface.AddPlacedText(
                    unit.context.currentUnit.Position, 
                    unit.context.SelectedAction ?? "No action", 
                    new Vec2(0,0),
                    1,
                    new Debugging.Color(1,0,0,1));
            }
        }

        public void Finish()
        {
        }


        public static UtilityAI<AIState> BuildAI(AIState state)
        {
            var reasoner = new HighestScoreReasoner<AIState>();

            var searching = new SumOfChildrenConsideration<AIState>();
            searching.Appraisals.Add(new EnemyVisible());
            searching.Action = new PursueEnemy();
            reasoner.AddConsideration(searching);

            reasoner.DefaultConsideration.Action = new MoveToCenter();

            return new UtilityAI<AIState>(state, reasoner);
        }

        private class NoEnemyVisible : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                var game = context.game;
                var closestEnemy = game.Units
                    .Where(a => a.PlayerId != game.MyId)
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .Take(1)
                    .Cast<Unit?>()
                    .FirstOrDefault();

                if (closestEnemy == null)
                {
                    return 100;
                }

                return 0;
            }
        }

        private class EnemyVisible : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                var game = context.game;
                var closestEnemy = game.Units
                    .Where(a => a.PlayerId != game.MyId)
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .Take(1)
                    .Cast<Unit?>()
                    .FirstOrDefault();

                if (closestEnemy != null)
                {
                    return 100;
                }

                return 0;
            }
        }

        private class MoveToCenter : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.SelectedAction = this.GetType().Name;
                var actionAim = new ActionOrder.Aim(true);
                if (actionAim == null)
                {
                    actionAim = new ActionOrder.Aim(false);
                }

                var game = context.game;
                var myUnit = context.currentUnit;

                var targetVelocity = game.Zone.CurrentCenter.Sub(myUnit.Position);
                var targetDirection = game.Zone.CurrentCenter.Sub(myUnit.Position);
                context.result = new UnitOrder(targetVelocity, targetDirection, actionAim);
            }
        }

        private class PursueEnemy : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.SelectedAction = this.GetType().Name;
                var actionAim = new ActionOrder.Aim(true);
                if (actionAim == null)
                {
                    actionAim = new ActionOrder.Aim(false);
                }

                var game = context.game;
                var myUnit = context.currentUnit;
                var closestEnemy = game.Units
                    .Where(a => a.PlayerId != game.MyId)
                    .OrderBy(a => a.Position.Sub(myUnit.Position).GetLengthQuad())
                    .Take(1)
                    .Cast<Unit?>()
                    .FirstOrDefault();

                var targetVelocity = closestEnemy.Value.Position.Sub(myUnit.Position);
                var targetDirection = closestEnemy.Value.Position.Sub(myUnit.Position);

                context.result = new UnitOrder(targetVelocity, targetDirection, actionAim);
            }
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
            return v1.Add(v2.X, v2.Y);
        }
        public static Vec2 Add(this Vec2 v1, double x, double y)
        {
            return new Vec2(v1.X + x, v1.Y + y);
        }
        public static Vec2 Sub(this Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X - v2.X, v1.Y - v2.Y);
        }
    }

    public class AIState
    {
        public Game game;
        public Unit currentUnit;
        public UnitOrder? result;
        public string SelectedAction;
    }
}