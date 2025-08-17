namespace Gameplay.Enemy
{
    /// <summary>
    /// Interface for enemy behaviours
    /// </summary>
    public interface IEnemyBehaviour
    {
        void PerformMovement(GridEnemy enemy);
        string GetBehaviourName();
        public bool TryCatchPlayer(GridEnemy enemy);
        public void OnTurnComplete(GridEnemy enemy);
    }
}
