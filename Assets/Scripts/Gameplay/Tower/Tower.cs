using Gameplay.Configs;
using Gameplay.MVP.Tower;
using TowerClicker.Infrastructure.Managers;
using UnityEngine;

namespace Gameplay
{
    public class Tower : BaseEntity
    {
        public TowerFloorManager TowerFloorManager => _towerFloorManager;
        
        [SerializeField] private ProgressionConfigSO progressionConfig;
        [SerializeField] private Transform visualRoot;
        
        private TowerFloorManager _towerFloorManager;
        private TowerModel _model;
        private TowerView _view;
        private TowerPresenter _presenter;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _towerFloorManager = GetComponent<TowerFloorManager>();
            _view = GetComponent<TowerView>();
            
            if (_view == null)
            {
                _view = gameObject.AddComponent<TowerView>();
            }
            
            _model = new TowerModel(_towerFloorManager, progressionConfig);
            _presenter = new TowerPresenter(_model, _view);
            
            _view.Initialize(_towerFloorManager);
            _presenter.Initialize();
        }
        
        public void OnHit(int damage)
        {
            _presenter.OnHit(damage);
        }
        
        private void OnDestroy()
        {
            _presenter?.Dispose();
        }
    }
}