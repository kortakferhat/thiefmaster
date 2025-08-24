using UnityEngine;

namespace Infrastructure.Managers.PoolManager
{
    public class ManagerBase : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}