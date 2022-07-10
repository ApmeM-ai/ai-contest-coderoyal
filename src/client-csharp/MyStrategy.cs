using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AiCup22.Model;
using AiCup22.UtilityAI.Appraisals;
using BrainAI.AI.UtilityAI.Actions;
using BrainAI.AI.UtilityAI.Reasoners;
using BrainAI.InfluenceMap;
using BrainAI.InfluenceMap.Fading;
using BrainAI.InfluenceMap.VectorGenerator;
using BrainAI.Pathfinding;
using static AiCup22.Model.Item;

namespace AiCup22
{
    public class MyStrategy
    {
        private readonly Constants constants;
        private readonly Reasoner<AIState> AI;
        private readonly CommunicationState communication = new CommunicationState();

        public MyStrategy(Constants constants)
        {
            Console.WriteLine(constants.InitialZoneRadius);
            this.constants = constants;
            this.AI = BuildAI();
            this.communication.map = new VectorInfluenceMap();
        }

        Dictionary<int, UnitState> units = new Dictionary<int, UnitState>();

        public Order GetOrder(Game game, DebugInterface debugInterface)
        {
            var result = new Dictionary<int, AiCup22.Model.UnitOrder>();

            var myUnits = game.Units.Where(a => a.PlayerId == game.MyId);

            this.communication.map.AddCharge(
                "border",
                new CircleChargeOrigin(new Point((int)game.Zone.CurrentCenter.X, (int)game.Zone.CurrentCenter.Y), (int)game.Zone.CurrentRadius),
                new LinearDistanceFading(50),
                -(float)(constants.UnitRadius * 2 + 2) * 50
            );

            this.communication.map.ClearLayer("bullet");
            foreach (var bullet in game.Projectiles)
            {
                if (bullet.ShooterPlayerId == game.MyId)
                {
                    continue;
                }

                this.communication.map.AddCharge(
                    "bullet",
                    new LineChargeOrigin(
                        new Point((int)bullet.Position.X, (int)bullet.Position.Y),
                        new Point((int)(bullet.Position.X + bullet.Velocity.X), (int)(bullet.Position.Y + bullet.Velocity.Y))
                        ),
                    new LinearDistanceFading(50),
                    -(float)(constants.UnitRadius * 2 + 2) * 50
                );
            }

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

        public void DebugUpdate(int displayedTick, DebugInterface debugInterface)
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


            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new HaveShieldPotionBool(),
                        new InvertBool(new ShieldPct()),
                        new CanHitEnemyBool(),
                        new FixedScoreAppraisal<AIState>(200)
                    ),
                new PrintAction("Прячусь!"),
                new SetMoveTargetToRandomPoint(),
                new MoveWithInfluenceMap(),
                new SetLookAtMoveTarget(),
                new TryPickBullet());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new HaveShieldPotionBool(),
                        new InvertBool(new ShieldPct()),
                        new InvertBool(new CanHitEnemyBool()),
                        new FixedScoreAppraisal<AIState>(160)
                    ),
                new PrintAction("Лечусь!"),
                new DrinkShield());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new NeedShieldPotionPct(),
                        new ShieldPotionVisibleBool(),
                        new FixedScoreAppraisal<AIState>(200)
                    ),
                new PrintAction("Нужна выпивка!"),
                new SetMoveTargetToPotion(),
                new MoveWithInfluenceMap(),
                new SetLookAtMoveTarget(),
                new TryPickBullet());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new EnemyVisibleBool(),
                        new InvertBool(new CanHitEnemyBool()),
                        new HaveBulletsBool(),
                        new FixedScoreAppraisal<AIState>(100)
                    ),
                new PrintAction("Догонялки!"),
                new SetMoveTargetToEnemy(),
                new MoveWithInfluenceMap(),
                new SetLookAtMoveTarget(),
                new TryPickBullet());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new EnemyVisibleBool(),
                        new CanHitEnemyBool(),
                        new HaveBulletsBool(),
                        new FixedScoreAppraisal<AIState>(100)
                    ),
                new PrintAction("Убиывлки!"),
                new SetMoveTargetToEnemy(),
                new MoveWithInfluenceMap(),
                new SetLookAtMoveTarget(),
                new Aim());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new FixedScoreAppraisal<AIState>(1)
                    ),
                new PrintAction("Заблудился!"),
                new SetMoveTargetToRandomPoint(),
                new MoveWithInfluenceMap(),
                new SetLookAtMoveTarget(),
                new TryPickBullet());

            return reasoner;
        }

        private class HealthPct : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                return (float)(context.currentUnit.Health / context.constants.UnitHealth);
            }
        }

        private class ShieldPct : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                return (float)(context.currentUnit.Shield / context.constants.MaxShield);
            }
        }

        private class EnemyVisibleBool : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                return context.game.Units
                    .Where(a => a.PlayerId != context.game.MyId)
                    .Any() ? 1 : 0;
            }
        }

        private class CanHitEnemyBool : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                return context.game.Units
                    .Where(a => a.PlayerId != context.game.MyId)
                    .Any() ? 1 : 0;
            }
        }

        private class InvertBool : IAppraisal<AIState>
        {
            private readonly IAppraisal<AIState> boolAppraisal;

            public InvertBool(IAppraisal<AIState> boolAppraisal)
            {
                this.boolAppraisal = boolAppraisal;
            }
            public float GetScore(AIState context)
            {
                return 1 - this.boolAppraisal.GetScore(context);
            }
        }

        private class ShieldPotionVisibleBool : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                return context.game.Loot
                    .Where(a => a.Item is ShieldPotions)
                    .Where(a => a.Position.WithinZone(context.game))
                    .Any() ? 1 : 0;
            }
        }

        private class NeedShieldPotionPct : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                var isMaxShield = (context.currentUnit.Shield == context.constants.MaxShield);
                var pct = context.currentUnit.ShieldPotions / context.constants.MaxShieldPotionsInInventory;
                return (float)((1 - pct) * (isMaxShield ? 0.75 : 1));
            }
        }

        private class HaveShieldPotionBool : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                if (context.currentUnit.ShieldPotions == 0)
                {
                    return 0;
                }

                return 1;
            }
        }
        
        private class HaveBulletsBool : IAppraisal<AIState>
        {
            public float GetScore(AIState context)
            {
                if (context.currentUnit.Ammo[context.currentUnit.Weapon.Value] == 0)
                {
                    return 0;
                }

                return 1;
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
                    1,
                    new Debugging.Color(1, 0, 0, 1));
            }
        }

        private class SetMoveTargetToRandomPoint : IAction<AIState>
        {
            private static Random r = new Random();

            public void Execute(AIState context)
            {
                if (context.unitState.RandomPoint.WithinZone(context.game))
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

        private class SetMoveTargetToEnemy : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                var closestEnemy = context.game.Units
                    .Where(a => a.PlayerId != context.game.MyId)
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .Cast<Unit>()
                    .First();

                context.unitState.MoveToPoint = closestEnemy.Position;
            }
        }

        private class SetMoveTargetToPotion : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                var closestPotion = context.game.Loot
                    .Where(a => a.Item is ShieldPotions)
                    .Where(a => a.Position.WithinZone(context.game))
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .First();

                context.debug.AddCircle(closestPotion.Position, 1, new Debugging.Color(0, 1, 1, 1.5));

                if (closestPotion.Position.Sub(context.currentUnit.Position).GetLengthQuad() < context.constants.UnitRadius * context.constants.UnitRadius)
                {
                    context.result = new UnitOrder(
                        context.result?.TargetVelocity ?? new Vec2(0, 0),
                        context.result?.TargetDirection ?? new Vec2(0, 0),
                        new ActionOrder.Pickup(closestPotion.Id));
                }

                context.unitState.MoveToPoint = closestPotion.Position;
            }
        }
        
        private class TryPickBullet : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                var closestAmmo = context.game.Loot
                    .Where(a => a.Item is Ammo)
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

        private class MoveWithInfluenceMap : IAction<AIState>
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
                // var constants = context.constants;
                // var debugInterface = context.debug;
                // var myUnit = context.currentUnit;
                // var communication = context.communicationState;

                // for (var x = -(int)constants.ViewDistance / 4; x < (int)constants.ViewDistance / 4; x++)
                //     for (var y = -(int)constants.ViewDistance / 4; y < (int)constants.ViewDistance / 4; y++)
                //     {
                //         var pos = new Point((int)myUnit.Position.X + x, (int)myUnit.Position.Y + y);
                //         var charge = communication.map.FindForceDirection(pos);
                //         var chargeLength = Math.Sqrt(charge.X * charge.X + charge.Y * charge.Y);
                //         if (chargeLength > 1)
                //         {
                //             debugInterface.AddPolyLine(
                //             new Vec2[] {
                //                 new Vec2(myUnit.Position.X + x, myUnit.Position.Y + y),
                //                 new Vec2(myUnit.Position.X + x + charge.X / chargeLength, myUnit.Position.Y + y + charge.Y / chargeLength),
                //                 },
                //             0.1,
                //             new Debugging.Color(1, 0, 0, 1));
                //         }
                //     }
                // context.debug?.AddPolyLine(
                //     new Vec2[] { context.currentUnit.Position, context.currentUnit.Position.Add(move) },
                //     0.5,
                //     new Debugging.Color(0, 1, 0, 1));

            }
        }

        private class SetLookAtNearestEnemy : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                var closestEnemy = context.game.Units
                    .Where(a => a.PlayerId != context.game.MyId)
                    .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                    .Cast<Unit?>()
                    .FirstOrDefault();

                if (closestEnemy == null)
                {
                    return;
                }

                context.result = new UnitOrder(
                    context.result?.TargetVelocity ?? new Vec2(0, 0),
                    closestEnemy.Value.Position.Sub(context.currentUnit.Position),
                    context.result?.Action);
            }
        }

        private class SetLookAtMoveTarget : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.result = new UnitOrder(
                    context.result?.TargetVelocity ?? new Vec2(0, 0),
                    context.unitState.MoveToPoint.Sub(context.currentUnit.Position),
                    context.result?.Action);
            }
        }

        private class Aim : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.result = new UnitOrder(
                    context.result?.TargetVelocity ?? new Vec2(0, 0),
                    context.result?.TargetDirection ?? new Vec2(0, 0),
                    new ActionOrder.Aim(true));
            }
        }

        private class DrinkShield : IAction<AIState>
        {
            public void Execute(AIState context)
            {
                context.result = new UnitOrder(
                    context.result?.TargetVelocity ?? new Vec2(0, 0),
                    context.result?.TargetDirection ?? new Vec2(0, 0),
                    new ActionOrder.UseShieldPotion());
            }
        }
    }

    public static class Extensions
    {
        public static bool WithinZone(this Vec2 v, Game game)
        {
            return v.Sub(game.Zone.CurrentCenter).GetLengthQuad() >
                    game.Zone.CurrentRadius * game.Zone.CurrentRadius;
        }
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
        public static Vec2 Normalize(this Vec2 v1)
        {
            var len = Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y);
            return v1.Div(len);
        }
        public static Vec2 Mul(this Vec2 v1, double value)
        {
            return new Vec2(v1.X * value, v1.Y * value);
        }
        public static Vec2 Div(this Vec2 v1, double value)
        {
            return new Vec2(v1.X / value, v1.Y / value);
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
        public Vec2 MoveToPoint;
        public Vec2 RandomPoint = new Vec2(0, 0);
    }

    public class CommunicationState
    {
        public VectorInfluenceMap map;
    }
}