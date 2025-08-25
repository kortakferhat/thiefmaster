namespace Infrastructure.Managers
{
    /// <summary>
    /// Interface for poolable objects
    /// </summary>
    public interface IPoolable
    {
        string PoolTag { get; }
        void OnSpawnFromPool();
        void OnDespawn();
    }
}