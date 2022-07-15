using System;
using System.Linq;
using AiCup22.Model;
using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class CanHitEnemyBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            if (!context.currentUnit.Weapon.HasValue)
            {
                return 0;
            }

            var knownEnemies = context.communicationState.EnemyMemory
                .Select(a => a.Item)
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad());

            var weaponData = context.constants.Weapons[context.currentUnit.Weapon.Value];
            var weaponRange = weaponData.ProjectileLifeTime * weaponData.ProjectileSpeed;

            var trees = context.communicationState.SpatialHash.GetAtRect(
                    (int)(context.currentUnit.Position.X - weaponRange),
                    (int)(context.currentUnit.Position.Y - weaponRange),
                    (int)(weaponRange * 2),
                    (int)(weaponRange * 2)
                    );

            foreach (var enemy in knownEnemies)
            {
                if (enemy.Position.Sub(context.currentUnit.Position).GetLengthQuad() > weaponRange * weaponRange)
                {
                    continue;
                }

                var p1 = context.currentUnit.Position;
                var p2 = enemy.Position;
                var vectToEnemy = p2.Sub(p1);
                var distanceToEnemy = p1.Sub(p2).GetLengthQuad();
                var treeIsOnTheWay = false;
                foreach (var tree in trees)
                {
                    var p0 = tree.Position;

                    var vectToTree = p0.Sub(p1);
                    
                    var alphaToTree = vectToEnemy.AngleBetweeVec(vectToTree);
                    if (alphaToTree > Math.PI / 2)
                    {
                        continue;
                    }

                    var distanceToTree = p1.Sub(p0).GetLengthQuad();
                    if (distanceToEnemy < distanceToTree)
                    {
                        continue;
                    }

                    var distance = Math.Abs((p2.Y - p1.Y) * p0.X - (p2.X - p1.X) * p0.Y + p2.X * p1.Y - p2.Y * p1.X) /
                                    Math.Sqrt((p2.Y - p1.Y) * (p2.Y - p1.Y) + (p2.X - p1.X) * (p2.X - p1.X));
                    if (distance < tree.Radius)
                    {
                        treeIsOnTheWay = true;
                        context.debug.AddCircle(tree.Position, tree.Radius, new Debugging.Color(0, 0, 0, 1));
                        break;
                    }
                }

                if (treeIsOnTheWay)
                {
                    continue;
                }

                var vectToShoot = context.currentUnit.Direction;
                var alphaToEnemy = vectToEnemy.AngleBetweeVec(vectToShoot);

                if (alphaToEnemy > weaponData.Spread * Math.PI / 360)
                {
                    continue;
                }

                return 1;
            }

            return 0;
        }
    }
}