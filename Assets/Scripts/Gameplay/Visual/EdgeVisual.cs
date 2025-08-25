using Infrastructure.Managers;
using UnityEngine;

namespace Gameplay.Visual
{
    public class EdgeVisual : MonoBehaviour, IPoolable
    {
        public string PoolTag { get; }
        [SerializeField] private string poolTag;
        
        public void OnSpawnFromPool()
        {
        }

        public void OnDespawn()
        {
        }
    }
}