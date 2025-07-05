using System;
using Gameplay.Configs;
using Gameplay.Events;
using Gameplay.Floors;
using Gameplay.MVP;
using Gameplay.Popups;
using Infrastructure;
using Infrastructure.Managers;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.MVP.Tower
{
    public class TowerPresenter : IPresenter
    {
        private readonly TowerModel _model;
        private readonly TowerView _view;
        
        private IGameManager _gameManager;
        private IParticleManager _particleManager;
        private IViewManager _viewManager;
        
        public TowerPresenter(TowerModel model, TowerView view)
        {
            _model = model;
            _view = view;
        }
        
        public void Initialize()
        {
            _gameManager = ServiceLocator.Get<IGameManager>();
            _particleManager = ServiceLocator.Get<IParticleManager>();
            _viewManager = ServiceLocator.Get<IViewManager>();
            
            // UI başlangıç değerlerini ayarla
            _view.UpdateExperienceBar(_model.GetExperienceToLevelUp(), 0);
            _view.UpdateLevelText(_model.Data.Level);
            
            // View Event Handlers
            _view.OnUpgradeButtonClicked += HandleUpgradeButtonClick;

            // Global Event Bus Subscriptions
            EventBus.Subscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
        }
        
        private void HandleUpgradeButtonClick()
        {
            if (_gameManager.State is not GameState.Game) return;
            //_viewManager.ShowPopupAsync<TowerUpgradePopup>("TowerUpgradePopup");
        }
        
        private void OnPlayerRewardCollect(PlayerRewardCollectEvent args)
        {
            if (_gameManager.State is not GameState.Game) return;
            
            _model.AddExperience(args.Amount);
            _view.SetExperienceValue(_model.Data.Experience, () =>
            {
                if (_model.IsLevelUpAvailable())
                {
                    ProcessLevelUp();
                }
            });
        }
        
        private void ProcessLevelUp()
        {
            _model.LevelUp();
            
            _view.UpdateExperienceBar(_model.GetExperienceToLevelUp(), 0);
            _view.UpdateLevelText(_model.Data.Level);
            
            EventBus.Publish(new TowerLevelUpEvent(_model.Data.Level, _model.Data.Experience));
        }
        
        public void OnHit(int damage)
        {
            if (_gameManager.State is not GameState.Game) return;
            
            bool shouldDie = _model.ProcessHit(damage);
            
            if (shouldDie)
            {
                Die();
                return;
            }
            
            _view.PlayHitAnimation();
        }
        
        private void Die()
        {
            _particleManager.PlayParticle(PoolKeys.TowerExplosionVFX, _view.transform.position, Quaternion.identity);
            _gameManager.EndGame();
            
            Dispose();
        }
        
        public void Dispose()
        {
            _view.OnUpgradeButtonClicked -= HandleUpgradeButtonClick;
            EventBus.Unsubscribe<PlayerRewardCollectEvent>(OnPlayerRewardCollect);
        }
    }
}