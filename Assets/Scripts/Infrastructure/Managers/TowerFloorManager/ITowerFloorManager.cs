using System.Collections.Generic;
using Gameplay.Floors;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace TowerClicker.Infrastructure.Managers
{
    public interface ITowerFloorManager : IService
    { 
        public int FloorCount { get; }
        public void Initialize();
        public void AddFloor(string poolType, int floorIndex);
        public void AddToTop(string poolType);
        public void RemoveFromBottom();
        public void RemoveFloor(int floorIndex);
        public List<TowerFloor> GetFloors();
        public TowerFloor GetFloor(int floorIndex, out TowerFloor towerFloor);
        public void UpgradeFloor(int floorIndex);
        public void DowngradeFloor(int floorIndex);
        public void SwapFloors(int floorIndexA, int floorIndexB);
        public TowerFloor GetBottomFloor();
        public int GetTotalFloorCount();
    }
}