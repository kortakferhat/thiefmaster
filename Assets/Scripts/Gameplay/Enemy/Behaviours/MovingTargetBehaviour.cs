using UnityEngine;
using Gameplay.Enemy;
using Gameplay.Events;
using Gameplay.Graph;
using Gameplay.Enemy.Behaviours;
using Infrastructure;

namespace Gameplay.Enemy.Behaviours
{
    /// <summary>
    /// MovingTarget behaviour - follows player if within 1 edge distance
    /// </summary>
    public class MovingTargetBehaviour : IEnemyBehaviour
    {
        public void PerformMovement(GridEnemy enemy)
        {
            var graph = enemy.LevelManager.GetCurrentGraph();
            
            // Check if player is within 1 edge distance
            if (enemy.IsPlayerInVision(enemy.CurrentPlayerNodeId))
            {
                // Player is visible - calculate direction to player
                var directionToPlayer = enemy.CurrentPlayerNodeId - enemy.CurrentNodeId;
                var targetNode = enemy.CurrentNodeId + directionToPlayer;
                
                if (graph.CanMoveFromTo(enemy.CurrentNodeId, targetNode))
                {
                    // Check if player is at target node
                    if (enemy.IsPlayerAtNode(targetNode))
                    {
                        // Player is at target node - move there and trigger game over
                        enemy.MoveToNode(targetNode);
                        EventBus.Publish(new LoseEvent(enemy.TurnManager.CurrentTurn, LoseReason.EnemyContact, targetNode));
                        return;
                    }
                    
                    // Move towards player
                    enemy.MoveToNode(targetNode);
                }
            }
            else
            {
                // Player is too far (more than 1 edge) - switch to stationary
                enemy.SetBehaviour(new StationaryBehaviour());
                if (enemy.ShowDebugLogs)
                    Debug.Log($"[GridEnemy] Player too far, switching to Stationary");
            }
        }
        
        public string GetBehaviourName() => "MovingTarget";
    }
}
