using Gameplay.Events;
using Gameplay.MainMenu;
using Gameplay.MVP;
using Gameplay.Popups;
using Infrastructure;
using Infrastructure.Managers;
using Infrastructure.Managers.EconomyManager;
using TowerClicker.Infrastructure;

namespace Gameplay.Views
{
    public class MainMenuPresenter : IPresenter
    {
        private readonly MainMenuModel _model;
        private readonly MainMenuView _view;

        private IGameManager _gameManager;
        private IViewManager _viewManager;
        private IEconomyManager _economyManager;
        
        public MainMenuPresenter(MainMenuModel model, MainMenuView view)
        {
            _model = model;
            _view = view;
        }
        
        public void Initialize()
        {
            _gameManager = ServiceLocator.Get<IGameManager>();
            _viewManager = ServiceLocator.Get<IViewManager>();
            _economyManager = ServiceLocator.Get<IEconomyManager>();
            
            //_view.OnTowerPopupButtonClicked += HandleTowerPopupButtonClick;
            
            EventBus.Subscribe<TowerLevelUpEvent>(OnTowerLevelUp);
            EventBus.Subscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
            EventBus.Subscribe<EconomyEvent>(OnEconomyEvent);
        }

        private void OnEconomyEvent(EconomyEvent args)
        {
            _model.SetMoney(args.CurrentMoney);
            _view.SetMoneyText(_model.Money);
        }

        private void OnPlayerRewardCollect(PlayerRewardCollectEvent args)
        {
            _model.AddMoney(args.Amount);
            _view.SetMoneyText(_model.Money);
            
            _economyManager.AddMoney(args.Amount);
        }

        private void OnTowerLevelUp(TowerLevelUpEvent args)
        {
            OpenTowerUpgradePopup();
        }

        private void HandleTowerPopupButtonClick()
        {
            OpenTowerUpgradePopup();
        }
        
        private void OpenTowerUpgradePopup()
        {
            if (_gameManager.State is not GameState.Game) return;
            //_viewManager.ShowPopupAsync<TowerUpgradePopup>("TowerUpgradePopup");
        }
        
        public void Dispose()
        {
            //_view.OnTowerPopupButtonClicked -= HandleTowerPopupButtonClick;
            EventBus.Unsubscribe<TowerLevelUpEvent>(OnTowerLevelUp);
            EventBus.Unsubscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
            EventBus.Unsubscribe<EconomyEvent>(OnEconomyEvent);
        }
    }
}