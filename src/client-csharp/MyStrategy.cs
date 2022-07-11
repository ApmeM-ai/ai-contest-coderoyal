using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AiCup22.Model;
using AiCup22.UtilityAI.Appraisals;
using BrainAI.AI.UtilityAI.Reasoners;
using BrainAI.InfluenceMap;
using BrainAI.InfluenceMap.Fading;
using BrainAI.InfluenceMap.VectorGenerator;
using BrainAI.Pathfinding;

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

            this.communication.map.ClearLayer("border");
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
                new ShowInfluenceMap().Execute(aiState);

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
                new TryPickBullet(),
                new TryPickShield());

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
                new SetMoveTargetToShield(),
                new MoveWithInfluenceMap(),
                new SetLookAtMoveTarget(),
                new TryPickBullet(),
                new TryPickShield());

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
                new TryPickBullet(),
                new TryPickShield());

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
                new SetLookScan(),
                new TryPickBullet(),
                new TryPickShield());

            return reasoner;
        }
   }

    public static class Extensions
    {
        public static bool WithinZone(this Vec2 v, Game game)
        {
            return v.Sub(game.Zone.CurrentCenter).GetLengthQuad() <
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