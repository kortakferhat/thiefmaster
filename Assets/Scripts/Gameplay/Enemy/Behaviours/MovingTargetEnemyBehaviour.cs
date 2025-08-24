using UnityEngine;

namespace Gameplay.Enemy.Behaviours
{
    /// <summary>
    /// MovingTarget behaviour - follows player if within 1 edge distance
    /// </summary>
    public class MovingTargetEnemyBehaviour : BaseEnemyBehaviour
    {
        public override void PerformMovement(GridEnemy enemy)
        {
            base.PerformMovement(enemy);
            // Check if player is within 1 edge distance
           
            var canCatchPlayer = TryCatchPlayer(enemy);
            if (canCatchPlayer)
            {
                return;
            }
            
            enemy.SetBehaviour(new StationaryEnemyBehaviour());
            if (enemy.ShowDebugLogs)
            {
                Debug.Log($"[GridEnemy] Player too far, switching to Stationary");
            }
            
            TryCatchPlayer(enemy); // Check after movement
        }
        
        public override string GetBehaviourName() => "MovingTarget";
    }
}
