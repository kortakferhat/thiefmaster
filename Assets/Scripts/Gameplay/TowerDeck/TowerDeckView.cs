using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Floors;
using Gameplay.MainMenu.TowerUpgrade;
using Gameplay.MVP;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.TowerDeck
{
    public class TowerDeckView : MonoBehaviour, IView
    {
        public event Action<TowerAddFloorButtonViewData> OnTowerAddFloorButtonClicked;

        [SerializeField] private Canvas towerDeckCanvas;
        [SerializeField] private HorizontalLayoutGroup towerDeckLayoutGroup;
        
        private IPoolManager _poolManager;
        
        public void Initialize()
        {
            _poolManager = ServiceLocator.Get<IPoolManager>();

            RefreshTowerDeck();
        }
        
        public async void RefreshTowerDeck()
        {
            towerDeckLayoutGroup.enabled = true;

            await UniTask.DelayFrame(1, PlayerLoopTiming.Update, destroyCancellationToken).SuppressCancellationThrow();
            
            towerDeckLayoutGroup.enabled = false;
        }

        public void AddFloorButtons(List<TowerAddFloorButtonViewData> floors)
        {
            foreach (var floor in floors)
            {
                var button = CreateFloorButton(floor);
                button.transform.SetParent(towerDeckLayoutGroup.transform, false);
                button.transform.localPosition = Vector3.zero;
                button.transform.localScale = Vector3.one;
                button.transform.localRotation = Quaternion.identity;
            }
        }

        private TowerAddFloorButton CreateFloorButton(TowerAddFloorButtonViewData floorButtonViewData)
        {
            var button = _poolManager.Spawn(PoolKeys.TowerAddFloorButton, towerDeckLayoutGroup.transform).GetComponent<TowerAddFloorButton>();
            button.Initialize(floorButtonViewData.DisplayName, floorButtonViewData.Price, floorButtonViewData.Icon);
            button.SetInteractable(true);
            button.AddOnClickListener(() =>
            {
                OnTowerAddFloorButtonClicked?.Invoke(floorButtonViewData);
            });
            
            return button;
        }
    }
}