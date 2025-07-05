using System;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Floors;
using Infrastructure.Managers.PoolManager;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerClicker.Infrastructure.Managers
{
    [CustomEditor(typeof(TowerFloorManager))]
    public class MyComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Add Top"))
            {
                var rnd = Random.Range(0, 5);

                var poolType = PoolKeys.TowerFloorTurret;
                switch (rnd)
                {
                    case 1: poolType = PoolKeys.TowerFloorBomb; break;
                    case 2: poolType = PoolKeys.TowerFloorOrb; break;
                    case 3: poolType = PoolKeys.TowerFloorWave; break;
                    case 4: poolType = PoolKeys.TowerFloorMirror; break;
                }
                ((TowerFloorManager)target).AddToTop(poolType);
            }
            
            if (GUILayout.Button("Remove Bottom"))
            {
                ((TowerFloorManager)target).RemoveFromBottom();
            }
        }
    }
    
    public class TowerFloorManager : MonoBehaviour, ITowerFloorManager
    {
        public int FloorCount => _towerFloors.Count;

        [SerializeField] private Transform floorsRoot;
        private float _towerHeight;
        private readonly int _maxFloors = 10;
        private IPoolManager _poolManager;
        private Vector3 towerFloorPositionOffset = new Vector3(0, 0.75f, 0);
        private readonly List<TowerFloor> _towerFloors = new ();
        
        public void Initialize()
        {
            _poolManager = ServiceLocator.Get<IPoolManager>();
            AddToTop(PoolKeys.TowerFloorTurret);
        }
        
        public void AddFloor(string poolType, int floorIndex)
        {
            if (FloorCount >= _maxFloors)
            {
                Debug.LogError($"Max floors reached: {_maxFloors}");
                return;
            }
            
            var floor = _poolManager.Spawn(poolType, Vector3.zero, Quaternion.identity).GetComponent<TowerFloor>();
            floor.transform.SetParent(floorsRoot);
            floor.transform.localPosition = new Vector3(0, floor.FloorData.Height * GetTotalFloorCount(), 0) + towerFloorPositionOffset;
            floor.Initialize();
            _towerFloors.Add(floor);
            _towerHeight += floor.FloorData.Height;

            var originalScale = floor.transform.localScale;
            var originalPosY = floor.transform.localPosition.y;
            
            floor.transform.localScale = originalScale * .25f;
            floor.transform.localPosition += Vector3.up * .45f;
            floor.transform.DOMoveY(originalPosY, .35f).SetEase(Ease.InBack).SetDelay(.25f);
            floor.transform.DOScale(originalScale, .25f).SetEase(Ease.OutBack);

            if (floorIndex < _towerFloors.Count)
            {
                var newFloor = _towerFloors[floorIndex];
                newFloor.transform.SetParent(floorsRoot);
                newFloor.transform.localPosition = new Vector3(0, _towerHeight, 0);
            }
        }
        
        public void AddToTop(string poolType)
        {
            if (FloorCount >= _maxFloors)
            {
                Debug.LogError($"Max floors reached: {_maxFloors}");
                return;
            }
            
            AddFloor(poolType, FloorCount + 1);
        }
        
        public void RemoveFromBottom()
        {
            if (FloorCount <= 0)
            {
                Debug.LogError("No floors to remove.");
                return;
            }
            
            RemoveFloor(0);
        }
        
        public void RemoveFloor(int floorIndex)
        {
            if (floorIndex < _towerFloors.Count)
            {
                var towerFloor = _towerFloors[floorIndex];
                _towerHeight -= towerFloor.FloorData.Height;

                _towerFloors.RemoveAt(floorIndex);
                _poolManager.Despawn(PoolKeys.TowerFloorTurret, towerFloor.gameObject);

                var currentFloorPosition = 0f;
                for (int i = 0; i < _towerFloors.Count; i++)
                {
                    var floor = _towerFloors[i];
                    floor.transform.localPosition = new Vector3(0, currentFloorPosition - floor.FloorData.Height, 0);
                    currentFloorPosition += floor.FloorData.Height;
                }
            }
            else
            {
                Debug.LogError($"Floor index {floorIndex} is out of range. Tower floor count: {_towerFloors.Count}");
            }
        }
        
        public List<TowerFloor> GetFloors()
        {
            return _towerFloors;
        }

        public TowerFloor GetFloor(int floorIndex, out TowerFloor towerFloor)
        {
            if (floorIndex < _towerFloors.Count)
            {
                towerFloor = _towerFloors[floorIndex];
                return towerFloor;
            }
            else
            {
                Debug.LogError($"Floor index {floorIndex} is out of range. Tower floor count: {_towerFloors.Count}");
                towerFloor = null;
                return null;
            }
        }
        
        public void UpgradeFloor(int floorIndex)
        {
            if (floorIndex < _towerFloors.Count)
            {
                var towerFloor = _towerFloors[floorIndex];
                towerFloor.Upgrade();
            }
            else
            {
                Debug.LogError($"Floor index {floorIndex} is out of range. Tower floor count: {_towerFloors.Count}");
            }
        }
        
        public void DowngradeFloor(int floorIndex)
        {
            if (floorIndex < _towerFloors.Count)
            {
                var towerFloor = _towerFloors[floorIndex];
                towerFloor.Downgrade();
            }
            else
            {
                Debug.LogError($"Floor index {floorIndex} is out of range. Tower floor count: {_towerFloors.Count}");
            }
        }
        
        public void SwapFloors(int floorIndexA, int floorIndexB)
        {
            if (floorIndexA < _towerFloors.Count && floorIndexB < _towerFloors.Count)
            {
                (_towerFloors[floorIndexA], _towerFloors[floorIndexB]) = (_towerFloors[floorIndexB], _towerFloors[floorIndexA]);

                // Update positions
                _towerFloors[floorIndexA].transform.localPosition = new Vector3(0, _towerHeight - floorIndexA, 0);
                _towerFloors[floorIndexB].transform.localPosition = new Vector3(0, _towerHeight - floorIndexB, 0);
            }
            else
            {
                Debug.LogError($"Floor index {floorIndexA} or {floorIndexB} is out of range. Tower floor count: {_towerFloors.Count}");
            }
        }

        public TowerFloor GetBottomFloor()
        {
            if (_towerFloors.Count > 0)
            {
                return _towerFloors[0];
            }
            else
            {
                Debug.LogError("No floors available.");
                return null;
            }
        }

        public int GetTotalFloorCount()
        {
            return _towerFloors.Count;
        }
    }
}