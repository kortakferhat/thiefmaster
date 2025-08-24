using UnityEngine;

namespace Infrastructure
{
    public interface IParticleManager : IService
    {
        public void Initialize(IPoolManager poolManager);
        public GameObject PlayParticle(string particleName, Vector3 position, Quaternion rotation);
    }
}