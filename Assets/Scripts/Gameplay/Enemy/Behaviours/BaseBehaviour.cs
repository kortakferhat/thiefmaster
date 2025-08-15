using Gameplay.Events;
using Infrastructure;

namespace Gameplay.Enemy.Behaviours
{
    public class BaseBehaviour : IEnemyBehaviour
    {
        public virtual void PerformMovement(GridEnemy enemy)
        {
        }

        public virtual string GetBehaviourName()
        {
            return "BaseBehaviour";
        }

        public bool TryCatchPlayer(GridEnemy enemy)
        {
            if (enemy.IsPlayerInVision(enemy.CurrentPlayerNodeId))
            {
                var directionToPlayer = enemy.CurrentPlayerNodeId - enemy.CurrentNodeId;
                var targetNode = enemy.CurrentNodeId + directionToPlayer;
              
                var graph = enemy.LevelManager.GetCurrentGraph();

                if (graph.CanMoveFromTo(enemy.CurrentNodeId, targetNode))
                {
                    if (enemy.IsPlayerAtNode(targetNode))
                    {
                        enemy.MoveToNode(targetNode);
                        EventBus.Publish(new LoseEvent(enemy.TurnManager.CurrentTurn, LoseReason.EnemyContact, targetNode));
                        return true;
                    }
                    
                    enemy.MoveToNode(targetNode);
                    return false;
                }
            }

            return false;
        }
    }
}