
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Infrastructure.Managers;
using UnityEngine.AddressableAssets;

namespace TowerClicker.Infrastructure
{
    public class PoolManager : MonoBehaviour, IPoolManager
    {
        // Object pool settings
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }
        
        [SerializeField] private PoolConfigurationSO poolConfiguration;
        [SerializeField] private Transform poolParent;

        private Dictionary<string, Queue<GameObject>> poolDictionary;
        
        
        public async Task Initialize()
        {
            poolConfiguration = await 
                Addressables.LoadAssetAsync<PoolConfigurationSO>("PoolConfiguration").ToUniTask(cancellationToken: destroyCancellationToken);

            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            
            // Create parent if needed
            if (poolParent == null)
            {
                poolParent = new GameObject("ObjectPools").transform;
                poolParent.SetParent(transform);
            }
            
            var allPools = new List<Pool>(poolConfiguration.pools);
            
            // Create each pool
            foreach (Pool pool in allPools)
            {
                // Create category parent
                GameObject categoryParent = new GameObject(pool.tag + "Pool");
                categoryParent.transform.SetParent(poolParent);
                
                Queue<GameObject> objectPool = new Queue<GameObject>();
                
                // Create and queue objects
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, categoryParent.transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                
                // Add pool to dictionary
                poolDictionary.Add(pool.tag, objectPool);
            }
        }
        
        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            // Check if the pool exists
            if (!poolDictionary.TryGetValue(tag, out var pool))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist");
                return null;
            }
            
            // Try to get an object from the pool

            if (pool.Count == 0)
            {
                Debug.LogWarning($"Pool with tag {tag} is empty");
                
                // Look for the prefab in both pool sources
                GameObject prefab = null;
                
                if (poolConfiguration != null)
                {
                    Pool configPoolMatch = poolConfiguration.pools.Find(p => p.tag == tag);
                    if (configPoolMatch != null)
                    {
                        prefab = configPoolMatch.prefab;
                    }
                }
                
                if (prefab == null)
                {
                    Debug.LogError($"Prefab for pool {tag} not found");
                    return null;
                }
                
                // Create a new object
                GameObject newObj = Instantiate(prefab, position, rotation);
                return newObj;
            }
            
            // Get the next object from the pool
            GameObject obj = pool.Dequeue();
            
            // Set position and rotation
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            // Activate the object
            obj.SetActive(true);
            
            // Get poolable component if it exists
            IPoolable poolable = obj.GetComponent<IPoolable>();
            if (poolable != null)
            {
                poolable.OnSpawnFromPool();
            }
            
            return obj;
        }

        public GameObject Spawn(string tag, Transform parent, Vector3 position, Quaternion rotation)
        {
            var go = Spawn(tag, position, rotation);
            if (go != null)
            {
                go.transform.SetParent(parent);
            }
            
            return go;
        }
        
        public GameObject Spawn(string tag, Transform parent, Vector3 scale, Vector3 position, Quaternion rotation)
        {
            var go = Spawn(tag, position, rotation);
            if (go != null)
            {
                go.transform.SetParent(parent);
                go.transform.localScale = scale;
            }
            
            return go;
        }
        
        public GameObject Spawn(string tag, Transform parent)
        {
            var go = Spawn(tag, Vector3.zero, Quaternion.identity);
            if (go != null)
            {
                go.transform.SetParent(parent);
            }
            
            return go;
        }

        public List<GameObject> GetActivePoolObjects(string tag)
        {
            List<GameObject> activeObjects = new List<GameObject>();

            if (poolDictionary == null)
            {
                Debug.LogWarning("Pool dictionary is null");
                return activeObjects;
            }
    
            // Check if the pool exists
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist");
                return activeObjects;
            }
    
            // Find the parent for this pool
            Transform poolCategoryParent = poolParent.Find(tag + "Pool");
            if (poolCategoryParent == null)
            {
                Debug.LogWarning($"Pool parent for tag {tag} not found");
                return activeObjects;
            }
    
            // Iterate through all children of the pool parent
            for (int i = 0; i < poolCategoryParent.childCount; i++)
            {
                GameObject obj = poolCategoryParent.GetChild(i).gameObject;
                if (obj.activeInHierarchy)
                {
                    activeObjects.Add(obj);
                }
            }
    
            return activeObjects;
        }
        
        public bool Despawn(string tag, GameObject obj)
        {
            // Check if the pool exists
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist. Object will be destroyed.");
                Destroy(obj);
                return false;
            }
            
            // Get a poolable component if it exists
            IPoolable poolable = obj.GetComponent<IPoolable>();
            if (poolable != null)
            {
                poolable.OnDespawn();
            }
            
            // Deactivate and return to pool
            obj.SetActive(false);
            poolDictionary[tag].Enqueue(obj);
            return true;
        }
        
        // Convenience method for delayed return
        public void DespawnAfterDelay(string tag, GameObject obj, float delay)
        {
            StartCoroutine(ReturnObjectAfterDelayCoroutine(tag, obj, delay));
        }
        
        private IEnumerator ReturnObjectAfterDelayCoroutine(string tag, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (obj != null && obj.activeInHierarchy)
            {
                Despawn(tag, obj);
            }
        }
        
        public void ClearPool(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                return;
            }
            
            Queue<GameObject> pool = poolDictionary[tag];
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                Destroy(obj);
            }
        }
        
        public void ClearAllPools()
        {
            foreach (var pool in poolDictionary.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    Destroy(obj);
                }
            }
            
            poolDictionary.Clear();
        }
    }
}