using System.Collections.Generic;
using Gameplay.Floors;
using Gameplay.MVP;
using TowerClicker.Infrastructure.Managers;

namespace Gameplay.TowerDeck
{
    public class TowerDeckModel : IModel
    {
        public ITowerFloorManager TowerFloorManager => _towerFloorManager;
        public List<TowerAddFloorButtonViewData> TowerDeckFloorViewData => _towerDeckFloorViewData;
        
        private List<TowerAddFloorButtonViewData> _towerDeckFloorViewData = new List<TowerAddFloorButtonViewData>();
        private ITowerFloorManager _towerFloorManager;
        
        public TowerDeckModel(ITowerFloorManager towerFloorManager, List<TowerFloorData> floorDeck) // TODO: Logic, ScriptableObject
        {
            _towerFloorManager = towerFloorManager;
            
            foreach (var floor in floorDeck)
            {
                _towerDeckFloorViewData.Add(new TowerAddFloorButtonViewData(floor.PoolKey, floor.Price, floor.Icon, true));
            }
        }
    }
}