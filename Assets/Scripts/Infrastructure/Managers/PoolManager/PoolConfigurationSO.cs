using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    [CreateAssetMenu(fileName = "PoolConfiguration", menuName = "TowerClicker/Pool Configuration", order = 1)]
    public class PoolConfigurationSO : ScriptableObject
    {
        public List<PoolManager.Pool> pools = new List<PoolManager.Pool>();
    }
}