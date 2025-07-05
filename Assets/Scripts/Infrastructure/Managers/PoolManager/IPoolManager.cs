using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TowerClicker.Infrastructure
{
    public interface IPoolManager : IService
    {
        Task Initialize();
        GameObject Spawn(string tag, Vector3 position, Quaternion rotation);
        GameObject Spawn(string tag, Transform parent, Vector3 position, Quaternion rotation);
        GameObject Spawn(string tag, Transform parent, Vector3 scale, Vector3 position, Quaternion rotation);
        GameObject Spawn(string tag, Transform parent);
        bool Despawn(string tag, GameObject obj);
        List<GameObject> GetActivePoolObjects(string tag);
        void DespawnAfterDelay(string tag, GameObject obj, float delay);
        void ClearPool(string tag);
    }
} 