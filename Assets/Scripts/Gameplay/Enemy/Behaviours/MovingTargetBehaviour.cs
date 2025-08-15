using UnityEngine;

namespace Gameplay.Enemy.Behaviours
{
    /// <summary>
    /// MovingTarget behaviour - follows player if within 1 edge distance
    /// </summary>
    public class MovingTargetBehaviour : BaseBehaviour
    {
        public override void PerformMovement(GridEnemy enemy)
        {
            // Check if player is within 1 edge distance
           
            var canCatchPlayer = TryCatchPlayer(enemy);
            if (canCatchPlayer)
            {
                return;
            }
            
            enemy.SetBehaviour(new StationaryBehaviour());
            if (enemy.ShowDebugLogs)
            {
                Debug.Log($"[GridEnemy] Player too far, switching to Stationary");
            }
            
            TryCatchPlayer(enemy); // Check after movement
        }
        
        public string GetBehaviourName() => "MovingTarget";
    }
}
