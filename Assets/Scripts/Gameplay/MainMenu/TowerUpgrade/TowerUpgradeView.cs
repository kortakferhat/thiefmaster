using Gameplay.MVP;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.MainMenu.TowerUpgrade
{
    public class TowerUpgradeView : MonoBehaviour, IView
    {
        [SerializeField] private Transform towerAddFloorButtonParent;
        [SerializeField] private VerticalLayoutGroup towerAddFloorButtonLayoutGroup;
        
        private IPoolManager _poolManager;

        public void Initialize(IPoolManager poolManager)
        {
            _poolManager = poolManager;
        }
        
        /// <summary>
        /// towerAddFloorButtonLayoutGroup'a yeni bir buton oluşturur
        /// </summary>
        /// <returns>Oluşturulan buton GameObject'i</returns>
        public GameObject CreateTowerAddFloorButton()
        {
            var buttonInstance = _poolManager.Spawn(PoolKeys.TowerAddFloorButton, towerAddFloorButtonLayoutGroup.transform);
            return buttonInstance;
        }
        
        /// <summary>
        /// Butonların yerleştirileceği parent transform'u döner
        /// </summary>
        public Transform GetTowerAddFloorButtonParent()
        {
            return towerAddFloorButtonParent;
        }
        
        /// <summary>
        /// Butonların yerleştirildiği layout group komponentini döner
        /// </summary>
        public VerticalLayoutGroup GetTowerAddFloorButtonLayoutGroup()
        {
            return towerAddFloorButtonLayoutGroup;
        }
    }
}