using Gameplay.MVP;
using Infrastructure.Managers.EconomyManager;
using Infrastructure.Managers.TooltipManager;
using TowerClicker.Infrastructure;

namespace Gameplay.TowerDeck
{
    public class TowerDeckPresenter : IPresenter
    {
        private readonly TowerDeckModel _model;
        private readonly TowerDeckView _view;
        
        private IEconomyManager _economyManager;
        private ITooltipManager _tooltipManager;
        
        public TowerDeckPresenter(TowerDeckView view, TowerDeckModel model)
        {
            _view = view;
            _model = model;
        }
        
        public void Initialize()
        {
            _economyManager = ServiceLocator.Get<IEconomyManager>();
            _tooltipManager = ServiceLocator.Get<ITooltipManager>();
            
            _view.Initialize();
            
            var floors = _model.TowerDeckFloorViewData;
            _view.AddFloorButtons(floors);
            _view.OnTowerAddFloorButtonClicked += OnTowerAddFloorButtonClicked;
        }

        private void OnTowerAddFloorButtonClicked(TowerAddFloorButtonViewData floorButtonViewData)
        {
            if (_economyManager.GetMoney() < floorButtonViewData.Price)
            {
                _tooltipManager.ShowTooltip("Not enough money!!!");
                return;
            }
            
            _model.TowerFloorManager.AddToTop(floorButtonViewData.FloorName);
            _economyManager.RemoveMoney(floorButtonViewData.Price);
        }
        
        public void Dispose()
        {
            _view.OnTowerAddFloorButtonClicked -= OnTowerAddFloorButtonClicked;
        }
    }
}