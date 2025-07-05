namespace Infrastructure.Managers
{
    /// <summary>
    /// Interface for poolable objects
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnDespawn();
    }
}