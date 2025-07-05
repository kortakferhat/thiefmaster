using System.Collections.Generic;
using Gameplay.Floors.Turret.Weapons;
using UnityEngine;

namespace Gameplay.Floors
{
    public class TowerFloor : MonoBehaviour, ITowerFloor
    {
        public TowerFloorData FloorData => _runtimeData;
        
        [SerializeField] private TowerFloorData configData;
        [SerializeField] private List<BaseWeapon> weapons;
        private TowerFloorData _runtimeData;

        private void Awake()
        {
            _runtimeData = Instantiate(configData);
        }

        public void Initialize()
        {
            foreach (var v in weapons)
            {
                v.Initialize();
            }
        }

        public void Upgrade()
        {
        }

        public void Downgrade()
        {
        }
        
        public bool DecreaseHealth(int damage)
        {
            configData.Health -= damage;
            if (configData.Health <= 0)
            {
                return true;
            }
            
            return false;
        }
    }
}