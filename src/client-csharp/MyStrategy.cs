using System;
using System.Collections.Generic;
using System.Linq;
using AiCup22.Model;
using AiCup22.UtilityAI.Appraisals;
using AiCup22.Utils;
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
        private const int LOOT_TICK_MEMORY = 32;
        private const int ENEMY_TICK_MEMORY = 32;

        private readonly Constants constants;
        private readonly Reasoner<AIState> AI;
        private readonly CommunicationState communication = new CommunicationState();

        public MyStrategy(Constants constants)
        {
            this.constants = constants;
            this.AI = BuildAI();
            this.communication.map = new VectorInfluenceMap();
            foreach (var tree in this.constants.Obstacles.Where(a => !a.CanShootThrough))
            {
                this.communication.SpatialHash.Register(tree, (int)(tree.Position.X - tree.Radius), (int)(tree.Position.Y - tree.Radius), (int)(tree.Radius * 2), (int)(tree.Radius * 2));
            }
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
                -(float)(constants.UnitRadius * 2 + 5) * 50
            );

            this.UpdateMemoryLoot(game);
            this.ShowMemoryLoot(debugInterface);
            this.UpdateMemoryBullet(game);
            this.ShowMemoryBullet(debugInterface);
            this.UpdateMemoryEnemy(game);
            this.ShowMemoryEnemy(debugInterface);

            foreach (var myUnit in myUnits)
            {
                if (!units.ContainsKey(myUnit.Id))
                {
                    units[myUnit.Id] = new UnitState();
                }

                var context = new AIState
                {
                    constants = this.constants,
                    game = game,
                    currentUnit = myUnit,
                    result = null,
                    debug = debugInterface,
                    communicationState = this.communication,
                    unitState = units[myUnit.Id]
                };

                this.AddBulletsToInfluenceMap(context);
                this.AddVisibleLootToInfluenceMap(context);
                AI.SelectBestAction(context).Execute(context);
                this.ShowInfluenceMap(context);
                this.UpdatePreviousUnit(context);

                result.Add(myUnit.Id, context.result.Value);
            }

            return new Order(result);
        }


        public void DebugUpdate(int displayedTick, DebugInterface debugInterface)
        {
        }

        public void Finish()
        {
        }

        public static Reasoner<AIState> BuildAI()
        {
            // ToDo:
            // Подбор оружия
            // Прячусь нормально прятаться
            // Приближаться чтобы была возможность увернуться
            // Группировать пульки от пулемета
            // Звук ниоткуда
            // Пить у края
            // Подбор оружия
            var reasoner = new HighestScoreReasoner<AIState>();

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new HaveShieldPotionBool(),                // Have shield potion
                        new InvertBool(new ShieldPct()),           // Not enough shield
                        new FixedScoreAppraisal<AIState>(300)
                    ),
                new PrintAction("Лечусь!"),
                new SetMoveTargetToRandomPoint(),
                // new TrySetMoveTargetFromEnemy(),
                new MoveWithInfluenceMap(),
                new SetLookScan(),
                new UseShieldPotion());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new CanHitEnemyBool(),                      // We can hit them
                        new MaxAimBool(),                           // Ready to shoot
                        new InvertBool(new ShootCooldownBool()),    // Weapon is not on cooldown
                        new HaveBulletsBool(),                      // We have bullets
                        new FixedScoreAppraisal<AIState>(250)
                    ),
                new PrintAction("Стреляю!"),
                new SetMoveTargetToEnemy(),
                new MoveWithInfluenceMap(),
                new SetLookAtMoveTarget(),
                new Shoot());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new ShouldPickupLootBool(),
                        new FixedScoreAppraisal<AIState>(250)
                    ),
                new PrintAction("Хватаю лут."),
                new SetMoveTargetToShield(),
                new MoveWithInfluenceMap(),
                new SetLookScan(),
                new PickupLoot());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new InvertBool(new HaveShieldPotionBool()), // Do not have shield potion
                        new InvertBool(new ShieldPct()),            // Not enough shield
                        new ShieldPotionVisibleBool(),              // Shield potion within sight range
                        new FixedScoreAppraisal<AIState>(200)
                    ),
                new PrintAction("Иду за выпивкой!"),
                new SetMoveTargetToShield(),
                new MoveWithInfluenceMap(),
                new SetLookScan(),
                new Aim());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new InvertBool(new HaveBulletsBool()),      // We do not have bullets for our weapon
                        new RequiredBulletVisibleBool(),            // We see ruired bullets
                        new FixedScoreAppraisal<AIState>(150)
                    ),
                new PrintAction("Иду за пульками!"),
                new SetMoveTargetToRequiredBullets(),
                new MoveWithInfluenceMap(),
                new SetLookScan(),
                new Aim());

            reasoner.Add(new MultiplyOfchildrenAppraisal<AIState>(
                        new EnemyKnownBool(),                       // Enemy is visible
                        new InvertBool(new CanHitEnemyBool()),      // We can not hit them
                        new HaveBulletsBool(),                      // We have bullets
                        new FixedScoreAppraisal<AIState>(100)
                    ),
                new PrintAction("Догонялки!"),
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
                new Aim());

            return reasoner;
        }

        private void ShowMemoryLoot(DebugInterface debugInterface)
        {
            if (debugInterface == null)
            {
                return;
            }

            foreach (var item in this.communication.LootMemory)
            {
                debugInterface.AddCircle(
                    item.Item.Position,
                    1,
                    new Debugging.Color(0, 1, 0, 0.5 * ((double)item.TimeLeft) / LOOT_TICK_MEMORY));
            }
        }

        private void ShowMemoryEnemy(DebugInterface debugInterface)
        {
            if (debugInterface == null)
            {
                return;
            }

            foreach (var item in this.communication.EnemyMemory)
            {
                debugInterface.AddCircle(
                    item.Item.Position,
                    constants.MaxUnitForwardSpeed / constants.TicksPerSecond * (ENEMY_TICK_MEMORY - item.TimeLeft),
                    new Debugging.Color(1, 1, 0, 0.5 * ((double)item.TimeLeft) / ENEMY_TICK_MEMORY));
            }
        }

        private void ShowMemoryBullet(DebugInterface debugInterface)
        {
            if (debugInterface == null)
            {
                return;
            }

            foreach (var item in this.communication.BulletMemory)
            {
                debugInterface.AddCircle(
                    item.Item.Position,
                    0.5,
                    new Debugging.Color(1, 0, 0, 0.5));
            }
        }

        private void UpdateMemoryBullet(Game game)
        {
            this.communication.BulletMemory = this.communication.BulletMemory
                .Where(a => a.TimeLeft > 0)
                .ExceptBy(game.Projectiles.Select(a => a.Id), a => a.Item.Id)
                .ToList();

            foreach (var old in this.communication.BulletMemory)
            {
                old.TimeLeft--;
                old.Item.Position = old.Item.Position.Add(old.Item.Velocity.Div(constants.TicksPerSecond));
                var isHitObstacle = constants.Obstacles
                        .Where(b => !b.CanShootThrough)
                        .Select(b => new ValueTuple<Vec2, double>(b.Position, b.Radius))
                        .Union(game.Units
                                .Select(b => new ValueTuple<Vec2, double>(b.Position, constants.UnitRadius)))
                        .Any(b => b.Item1.Sub(old.Item.Position).GetLengthQuad() < b.Item2 * b.Item2);
                if (isHitObstacle)
                {
                    old.TimeLeft = 0;
                }
            }
            foreach (var added in game.Projectiles)
            {
                var timeLeft = added.LifeTime * constants.TicksPerSecond;
                this.communication.BulletMemory.Add(new Memory<Projectile>(added, (int)timeLeft));
            }
        }

        private void UpdateMemoryEnemy(Game game)
        {
            this.communication.EnemyMemory = this.communication.EnemyMemory
                .Where(a => a.TimeLeft > 0)
                .ExceptBy(game.Units.Select(a => a.Id), a => a.Item.Id)
                .ToList();

            foreach (var old in this.communication.EnemyMemory)
            {
                old.TimeLeft--;
            }
            foreach (var added in game.Units.Where(a => a.PlayerId != game.MyId))
            {
                this.communication.EnemyMemory.Add(new Memory<Unit>(added, ENEMY_TICK_MEMORY));
            }

            var allSounds = constants.Weapons
                .Select(a => a.ShotSoundTypeIndex)
                .Union(new[] { constants.StepsSoundTypeIndex })
                .ToHashSet();

            var soundPositions = game.Sounds.Where(a => allSounds.Contains(a.TypeIndex)).Select(a => a.Position);
            foreach (var sound in soundPositions)
            {
                foreach (var enemySound in this.communication.EnemyMemory
                    .Where(a =>
                        a.Item.Position.Sub(sound).GetLengthQuad() <
                        constants.MaxUnitForwardSpeed / constants.TicksPerSecond * (ENEMY_TICK_MEMORY - a.TimeLeft) *
                        constants.MaxUnitForwardSpeed / constants.TicksPerSecond * (ENEMY_TICK_MEMORY - a.TimeLeft)))
                {
                    enemySound.TimeLeft = ENEMY_TICK_MEMORY;
                }
            }
        }

        private void UpdateMemoryLoot(Game game)
        {
            this.communication.LootMemory = this.communication.LootMemory
                .Where(a => a.TimeLeft > 0)
                .ExceptBy(game.Loot.Select(a => a.Id), a => a.Item.Id)
                .ToList();

            foreach (var old in this.communication.LootMemory)
            {
                old.TimeLeft--;
            }
            foreach (var added in game.Loot)
            {
                this.communication.LootMemory.Add(new Memory<Loot>(added, LOOT_TICK_MEMORY));
            }
        }

        private void UpdatePreviousUnit(AIState aiState)
        {
            aiState.unitState.PreviousUnit = aiState.currentUnit;
        }

        private void AddBulletsToInfluenceMap(AIState context)
        {
            context.communicationState.map.ClearLayer("bullet");
            foreach (var bulletMemory in context.communicationState.BulletMemory)
            {
                var bullet = bulletMemory.Item;

                var vectorToMe = context.currentUnit.Position.Sub(bullet.Position);
                var angleToHit = vectorToMe.AngleBetweeVec(bullet.Velocity);
                if (angleToHit > Math.PI / 4)
                {
                    continue;
                }

                var distanceToMe = vectorToMe.GetLengthQuad();
                if (distanceToMe > bullet.Velocity.Mul(bullet.LifeTime).GetLengthQuad())
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
        }

        private void AddVisibleLootToInfluenceMap(AIState context)
        {
            this.communication.map.ClearLayer("loot");
            foreach (var lootMemory in context.communicationState.LootMemory)
            {
                var loot = lootMemory.Item;

                if ((loot.Item is not Ammo || ((Ammo)loot.Item).WeaponTypeIndex != context.currentUnit.Weapon.Value || context.currentUnit.Ammo[context.currentUnit.Weapon.Value] >= context.constants.Weapons[context.currentUnit.Weapon.Value].MaxInventoryAmmo) &&
                    (loot.Item is not ShieldPotions || context.currentUnit.ShieldPotions >= context.constants.MaxShieldPotionsInInventory))
                {
                    continue;
                }

                this.communication.map.AddCharge(
                    "loot",
                    new PointChargeOrigin(new Point((int)loot.Position.X, (int)loot.Position.Y)),
                    new LinearDistanceFading(10),
                    (float)(constants.UnitRadius * 2 + 2) * 10
                );
            }
        }

        private void ShowInfluenceMap(AIState context)
        {
            if (context.debug == null)
            {
                return;
            }

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
        public static double GetLength(this Vec2 v)
        {
            return Math.Sqrt(v.GetLengthQuad());
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
        public static double AngleBetweeVec(this Vec2 v1, Vec2 vec2)
        {
            return Math.Acos((v1.X * vec2.X + v1.Y * vec2.Y) / (v1.GetLength() * vec2.GetLength()));
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
        public Unit PreviousUnit;
    }

    public class CommunicationState
    {
        public VectorInfluenceMap map;
        public List<Memory<Loot>> LootMemory = new List<Memory<Loot>>();
        public List<Memory<Unit>> EnemyMemory = new List<Memory<Unit>>();
        public List<Memory<Projectile>> BulletMemory = new List<Memory<Projectile>>();
        public SpatialHash<Obstacle> SpatialHash = new SpatialHash<Obstacle>(20);
    }

    public class Memory<T>
    {
        public Memory(T item, int timeLeft)
        {
            Item = item;
            TimeLeft = timeLeft;
        }

        public T Item;
        public int TimeLeft;
    }
}