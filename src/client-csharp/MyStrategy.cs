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
        private readonly Reasoner<AIState> AI;
        private readonly CommunicationState communication = new CommunicationState();

        public MyStrategy(Constants constants)
        {
            this.constants = constants;
            this.AI = BuildAI();
        }

        Dictionary<int, UnitState> units = new Dictionary<int, UnitState>();

        public Order GetOrder(Game game, DebugInterface debugInterface)
        {
            var result = new Dictionary<int, AiCup22.Model.UnitOrder>();

            var myUnits = game.Units.Where(a => a.PlayerId == game.MyId);

            foreach (var myUnit in myUnits)
            {
                if (!units.ContainsKey(myUnit.Id))
                {
                    units[myUnit.Id] = new UnitState();
                }

                var aiState = new AIState
                {
                    constants = constants,
                    game = game,
                    currentUnit = myUnit,
                    result = null,
                    debug = debugInterface,
                    communicationState = communication,
                    unitState = units[myUnit.Id]
                };

                AI.SelectBestAction(aiState).Execute(aiState);

                result.Add(myUnit.Id, aiState.result.Value);
            }

            return new Order(result);
        }

        public void DebugUpdate(DebugInterface debugInterface)
        {
        }

        public void Finish()
        {
        }

/// Враг далеко, жизней много - подойти
/// Жизней мало и есть аптечки - применить
/// Врагов нет и видна аптечка - подобрать
/// Враг есть и жизней мало и нет аптечки - убегать и искать аптечку
        public static Reasoner<AIState> BuildAI()
        {
            var reasoner = new HighestScoreReasoner<AIState>();

            reasoner.Add(new EnemyVisible(), 
                new PrintAction("Догонялки"),
                new PursueEnemy());
            
            reasoner.Add(new FixedScoreAppraisal<AIState>(), 
                new PrintAction("Заблудился"), 
                new MoveToCenter());

            return reasoner;
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

        private class PrintAction : IAction<AIState>
        {
            private string action;

            public PrintAction(string action)
            {
                this.action = action;
            }

            public void Execute(AIState context)
            {
                context.debug?.AddPlacedText(
                    context.currentUnit.Position,
                    action,
                    new Vec2(0, 0),
                    5,
                    new Debugging.Color(1, 0, 0, 1));
            }
        }

        private class MoveToCenter : IAction<AIState>
        {
            public void Execute(AIState context)
            {
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
        public Constants constants;
        public Game game;
        public DebugInterface debug;
        public Unit currentUnit;
        public UnitState unitState;
        public CommunicationState communicationState;
        public UnitOrder? result;
    }

    public class UnitState
    {
    }

    public class CommunicationState
    {
    }
}