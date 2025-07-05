using Gameplay.Configs;
using Gameplay.Floors;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure.Managers;

namespace Gameplay.MVP.Tower
{
    public class TowerModel : IModel
    {
        public TowerData Data { get; private set; } = new TowerData();
        public int TotalFloorCount => _towerFloorManager.GetTotalFloorCount();
        
        private readonly ITowerFloorManager _towerFloorManager;
        private readonly ProgressionConfigSO _progressionConfig;

        public TowerModel(ITowerFloorManager towerFloorManager, ProgressionConfigSO progressionConfig)
        {
            _towerFloorManager = towerFloorManager;
            _progressionConfig = progressionConfig;
        }

        public int GetExperienceToLevelUp()
        {
            return _progressionConfig.GetExperienceToLevelUp(Data.Level);
        }

        public void AddExperience(int amount)
        {
            Data.Experience += amount;
        }

        public bool IsLevelUpAvailable()
        {
            return Data.Experience >= _progressionConfig.GetExperienceToLevelUp(Data.Level);
        }

        public void LevelUp()
        {
            Data.Level++;
            Data.Experience = 0;
        }

        public bool ProcessHit(int damage)
        {
            var bottomFloor = _towerFloorManager.GetBottomFloor();
            if (bottomFloor == null) return true;

            var shouldDestroy = bottomFloor.DecreaseHealth(damage);
            if (shouldDestroy)
            {
                _towerFloorManager.RemoveFromBottom();
            }

            return TotalFloorCount <= 0;
        }

        public void AddFloorToTop(string poolType)
        {
            _towerFloorManager.AddToTop(poolType);
        }
        
        public TowerFloor GetBottomFloor()
        {
            return _towerFloorManager.GetBottomFloor();
        }
    }
}