using UnityEngine;

namespace Gameplay.TowerDeck
{
    public struct TowerAddFloorButtonViewData
    {
        public string FloorName { get; }
        public string DisplayName => FloorName.Replace("TowerFloor", "");
        public int Price { get; }
        public Sprite Icon { get; }
        public bool IsInteractable { get; }

        public TowerAddFloorButtonViewData(string floorName, int price, Sprite icon, bool isInteractable)
        {
            FloorName = floorName;
            Price = price;
            IsInteractable = isInteractable;
            Icon = icon;
        }
    }
}