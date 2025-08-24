using Cysharp.Threading.Tasks;
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
        private ITurnManager _turnManager;
        
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
            _turnManager = ServiceLocator.Get<ITurnManager>();
            
            EventBus.Subscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
            EventBus.Subscribe<PlayerItemCollectEvent>(OnPlayerItemCollect);
            EventBus.Subscribe<EconomyEvent>(OnEconomyEvent);
            EventBus.Subscribe<TurnCompletedEvent>(OnTurnCompleted);
            EventBus.Subscribe<GameEvents.GameStateChangeEvent>(OnGameStateChange);
            
            _view.SetRemainingMovesText(_turnManager.RemainingMoves);
        }

        private void OnGameStateChange(GameEvents.GameStateChangeEvent args)
        {
            _view.PrepareGameStateChange(args.CurrentState);
        }

        private void OnTurnCompleted(TurnCompletedEvent args)
        {
            _view.SetRemainingMovesText(args.RemainingMoves);
        }

        private void OnEconomyEvent(EconomyEvent args)
        {
            _model.SetMoney(args.CurrentMoney);
        }
        
        private void OnPlayerItemCollect(PlayerItemCollectEvent args)
        {
            _model.AddItem(args.ItemType);
        }

        private void OnPlayerRewardCollect(PlayerRewardCollectEvent args)
        {
            _model.AddMoney(args.Amount);
            
            _economyManager.AddMoney(args.Amount);
        }
        
        private void OpenTowerUpgradePopup()
        {
            if (_gameManager.State is not GameState.Game) return;
        }
        
        public void Dispose()
        {
            EventBus.Unsubscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
            EventBus.Unsubscribe<PlayerItemCollectEvent>(OnPlayerItemCollect);
            EventBus.Unsubscribe<EconomyEvent>(OnEconomyEvent);
            EventBus.Unsubscribe<TurnCompletedEvent>(OnTurnCompleted);
            EventBus.Unsubscribe<GameEvents.GameStateChangeEvent>(OnGameStateChange);
        }
    }
}