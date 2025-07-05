using Infrastructure.Managers.PoolManager;
using Gameplay.Projectiles;
using UnityEngine;

namespace Gameplay.Projectiles
{
    public class Bullet : BaseBullet
    {
        protected override string GetDefaultPoolKey()
        {
            return PoolKeys.Bullet;
        }

        protected override void OnEnemyHit(Collider enemyCollider, Gameplay.Enemy.Enemy enemy)
        {
            // Standard bullet despawns after hitting any enemy
            Despawn();
        }
    }
} 