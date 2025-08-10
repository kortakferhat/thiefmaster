using UnityEngine;
using Gameplay.Enemy;
using Gameplay.Events;
using Gameplay.Graph;
using Infrastructure;

namespace Gameplay.Enemy.Behaviours
{
    /// <summary>
    /// Patrol behaviour - enemy moves back and forth
    /// </summary>
    public class PatrolBehaviour : IEnemyBehaviour
    {
        public void PerformMovement(GridEnemy enemy)
        {
            var graph = enemy.LevelManager.GetCurrentGraph();
            var targetNode = enemy.CurrentNodeId + enemy.FacingDirection;
            
            // Check if we can move in facing direction
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
                
                // Move to target node
                enemy.MoveToNode(targetNode);
            }
            else
            {
                // Cannot move in facing direction, reverse direction
                enemy.ReverseFacingDirection();
                
                // Try to move in new direction
                var newTargetNode = enemy.CurrentNodeId + enemy.FacingDirection;
                if (graph.CanMoveFromTo(enemy.CurrentNodeId, newTargetNode))
                {
                    // Check if player is at new target node
                    if (enemy.IsPlayerAtNode(newTargetNode))
                    {
                        // Player is at target node - move there and trigger game over
                        enemy.MoveToNode(newTargetNode);
                        EventBus.Publish(new LoseEvent(enemy.TurnManager.CurrentTurn, LoseReason.EnemyContact, newTargetNode));
                        return;
                    }
                    
                    // Move to new target node
                    enemy.MoveToNode(newTargetNode);
                }
            }
        }
        
        public string GetBehaviourName() => "Patrol";
    }
}
