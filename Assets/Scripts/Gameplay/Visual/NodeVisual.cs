using Infrastructure.Managers;
using UnityEngine;

namespace Gameplay.Visual
{
    public class NodeVisual : MonoBehaviour, IPoolable
    {
        public string PoolTag => poolTag;
        
        [SerializeField] private string poolTag;
        public void OnSpawnFromPool()
        {
        }

        public void OnDespawn()
        {
        }
    }
}