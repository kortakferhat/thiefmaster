using System.Collections.Generic;
using Gameplay.Events;
using Gameplay.MainMenu.TowerUpgrade;
using Gameplay.MVP;
using Infrastructure;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;

namespace Gameplay.Views
{
    public class TowerUpgradePresenter : IPresenter
    {
        private readonly TowerUpgradeModel _model;
        private readonly TowerUpgradeView _view;

        private IPoolManager _poolManager;
        
        private List<TowerAddFloorButton> _towerAddFloorButtons = new ();
        
        public TowerUpgradePresenter(TowerUpgradeModel model, TowerUpgradeView view)
        {
            _model = model;
            _view = view;
        }
        
        public void Initialize()
        {
            _view.Initialize(_poolManager);
            
            EventBus.Subscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
            RefreshTowerFloorButtons();
        }

        private void OnPlayerRewardCollect(PlayerRewardCollectEvent args)
        {
        }
        
        private void RefreshTowerFloorButtons()
        {
        }

        private void OnTowerAddButtonClick()
        {
            
        }

        private void ClearTowerFloorButtons()
        {
            foreach (var button in _towerAddFloorButtons)
            {
                button.RemoveOnClickListener(OnTowerAddButtonClick);
                button.gameObject.SetActive(false);

                _poolManager.Despawn(PoolKeys.TowerAddFloorButton, button.gameObject);
            }
            
            _towerAddFloorButtons.Clear();
        }
 
        public void Dispose()
        {
            ClearTowerFloorButtons();
            EventBus.Unsubscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
        }
    }
}