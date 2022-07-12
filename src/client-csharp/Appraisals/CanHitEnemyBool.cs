using System.Linq;
using AiCup22.Model;
using AiCup22.UtilityAI.Appraisals;

namespace AiCup22
{
    public class CanHitEnemyBool : IAppraisal<AIState>
    {
        public float GetScore(AIState context)
        {

            var closestEnemy = context.game.Units
                .Where(a => a.PlayerId != context.game.MyId)
                .OrderBy(a => a.Position.Sub(context.currentUnit.Position).GetLengthQuad())
                .Cast<Unit?>()
                .FirstOrDefault();

            if (closestEnemy == null)
            {
                return 0;
            }

            var weaponData = context.constants.Weapons[context.currentUnit.Weapon.Value];
            if (closestEnemy.Value.Position.Sub(context.currentUnit.Position).GetLengthQuad() >
                weaponData.ProjectileLifeTime * weaponData.ProjectileSpeed * weaponData.ProjectileLifeTime * weaponData.ProjectileSpeed)
            {
                return 0;
            }

            return 1;
        }
    }
}