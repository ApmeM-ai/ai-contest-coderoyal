using System.Linq;
using AiCup22.Model;
using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class EnemyCanHitBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {
            var visibleEnemies = context.game.Units
                .Where(a => a.PlayerId != context.game.MyId)
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad());

            foreach (var closestEnemy in visibleEnemies)
            {
                if (closestEnemy.Weapon == null)
                {
                    continue;
                }


                // ToDo: calculate safe distance based on weapon characteristics.
                float safeDistance = 2;
                var weaponData = context.constants.Weapons[closestEnemy.Weapon.Value];
                if (closestEnemy.Position.Sub(context.currentUnit.Position).GetLengthQuad() <
                    (weaponData.ProjectileLifeTime * weaponData.ProjectileSpeed + safeDistance) * 
                    (weaponData.ProjectileLifeTime * weaponData.ProjectileSpeed + safeDistance))
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}