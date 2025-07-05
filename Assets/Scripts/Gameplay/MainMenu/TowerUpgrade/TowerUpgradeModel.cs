using Gameplay.MVP;
using System.Collections.Generic;

namespace Gameplay.MainMenu.TowerUpgrade
{
    public class TowerUpgradeModel : IModel
    {
        private readonly Dictionary<int, FloorData> _availableFloors = new Dictionary<int, FloorData>();
        private readonly HashSet<int> _purchasedFloors = new HashSet<int>();
        private int _maxFloorCount = 10;
        
        public class FloorData
        {
            public string Name { get; set; }
            public int Price { get; set; }
            public string Type { get; set; }
            
            public FloorData(string name, int price, string type)
            {
                Name = name;
                Price = price;
                Type = type;
            }
        }
        
        public void AddAvailableFloor(int floorIndex, string name, int price, string type)
        {
            if (!_availableFloors.ContainsKey(floorIndex))
            {
                _availableFloors[floorIndex] = new FloorData(name, price, type);
            }
        }
        
        public void RemoveAvailableFloor(int floorIndex)
        {
            if (_availableFloors.ContainsKey(floorIndex))
            {
                _availableFloors.Remove(floorIndex);
            }
        }
        
        public Dictionary<int, FloorData> GetAvailableFloors()
        {
            return _availableFloors;
        }
        
        public bool IsFloorAvailable(int floorIndex)
        {
            return _availableFloors.ContainsKey(floorIndex);
        }
        
        public int GetFloorPrice(int floorIndex)
        {
            if (_availableFloors.TryGetValue(floorIndex, out FloorData data))
            {
                return data.Price;
            }
            return 0;
        }
        
        public string GetFloorType(int floorIndex)
        {
            if (_availableFloors.TryGetValue(floorIndex, out FloorData data))
            {
                return data.Type;
            }
            return string.Empty;
        }
        
        public string GetFloorName(int floorIndex)
        {
            if (_availableFloors.TryGetValue(floorIndex, out FloorData data))
            {
                return data.Name;
            }
            return $"Floor {floorIndex}";
        }
        
        public void PurchaseFloor(int floorIndex)
        {
            if (_availableFloors.ContainsKey(floorIndex) && !_purchasedFloors.Contains(floorIndex))
            {
                _purchasedFloors.Add(floorIndex);
            }
        }
        
        public bool IsFloorPurchased(int floorIndex)
        {
            return _purchasedFloors.Contains(floorIndex);
        }
        
        public int GetAvailableFloorCount()
        {
            return _availableFloors.Count;
        }
        
        public int GetPurchasedFloorCount()
        {
            return _purchasedFloors.Count;
        }
        
        public bool CanAddMoreFloors()
        {
            return GetPurchasedFloorCount() < _maxFloorCount;
        }
        
        public void Reset()
        {
            _availableFloors.Clear();
            _purchasedFloors.Clear();
        }
    }
}