using Infrastructure.Managers.PoolManager;
using UnityEngine;

namespace Gameplay.Floors
{
    [CreateAssetMenu(fileName = "TowerFloorData", menuName = "TowerClicker/Tower Floor Data", order = 3)]
    public class TowerFloorData : ScriptableObject
    {
        public string PoolKey => $"{PoolKeys.TowerFloorPrefix}{FloorType}";
        
        public TowerFloorType FloorType;
        public int Health = 3; // TODO: Make it configurable
        public int Price = 1; // TODO: Make it configurable
        public float Height = 1; // TODO: Make it configurable
        public Sprite Icon;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                Debug.LogWarning("OnValidate should not be called in play mode. Please use Awake or Start for runtime initialization.");
        }
#endif
    }
}