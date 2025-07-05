using UnityEngine;

namespace TowerClicker.Infrastructure
{
    public class ParticleManager : MonoBehaviour, IParticleManager
    {
        private IPoolManager _poolManager;
        public void Initialize(IPoolManager poolManager)
        {
            _poolManager = poolManager;
        }
        
        public GameObject PlayParticle(string particleName, Vector3 position, Quaternion rotation)
        {
            // Spawn hit VFX
            var hitVFX = _poolManager.Spawn(particleName, position, rotation);
            var ps = hitVFX.GetComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
            
            _poolManager.DespawnAfterDelay(particleName, hitVFX, ps.main.duration + 1);

            return hitVFX;
        }
    }
}