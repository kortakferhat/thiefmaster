using UnityEngine;
using Gameplay.Enemy;

namespace Gameplay.Enemy.Behaviours
{
    /// <summary>
    /// Stationary behaviour - enemy doesn't move
    /// </summary>
    public class StationaryEnemyBehaviour : BaseEnemyBehaviour
    {
        public override void PerformMovement(GridEnemy enemy)
        {
            base.PerformMovement(enemy);

            // Stationary enemies don't move
            if (enemy.ShowDebugLogs)
            {
                Debug.Log($"[GridEnemy] Stationary - no movement");
            }
        }
        
        public string GetBehaviourName() => "Stationary";
    }
}
