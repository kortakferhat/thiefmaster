using Gameplay.Events;
using Infrastructure;

namespace Gameplay.Enemy.Behaviours
{
    /// <summary>
    /// Patrol behaviour - enemy moves back and forth
    /// </summary>
    public class PatrolBehaviour : BaseBehaviour
    {
        public override void PerformMovement(GridEnemy enemy)
        {
            var graph = enemy.LevelManager.GetCurrentGraph();
            var targetNode = enemy.CurrentNodeId + enemy.FacingDirection;
            
            // Check if we can move in facing direction
            if (graph.CanMoveFromTo(enemy.CurrentNodeId, targetNode))
            {
                var canCatchPlayer = TryCatchPlayer(enemy);
                if (canCatchPlayer)
                {
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
                    var canCatchPlayer = TryCatchPlayer(enemy);
                    if (canCatchPlayer)
                    {
                        return;
                    }
                    
                    // Move to new target node
                    enemy.MoveToNode(newTargetNode);
                }
            }
            
            TryCatchPlayer(enemy); // Check after movement
        }
        
        public string GetBehaviourName() => "Patrol";
    }
}
