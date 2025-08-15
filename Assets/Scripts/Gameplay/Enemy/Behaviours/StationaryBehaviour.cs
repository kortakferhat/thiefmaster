using UnityEngine;
using Gameplay.Enemy;

namespace Gameplay.Enemy.Behaviours
{
    /// <summary>
    /// Stationary behaviour - enemy doesn't move
    /// </summary>
    public class StationaryBehaviour : BaseBehaviour
    {
        public override void PerformMovement(GridEnemy enemy)
        {
            // Stationary enemies don't move
            if (enemy.ShowDebugLogs)
            {
                Debug.Log($"[GridEnemy] Stationary - no movement");
            }
        }
        
        public string GetBehaviourName() => "Stationary";
    }
}
