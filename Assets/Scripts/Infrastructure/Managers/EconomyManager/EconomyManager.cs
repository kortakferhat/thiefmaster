using Gameplay.Events;
using UnityEngine;

namespace Infrastructure.Managers.EconomyManager
{
    public class EconomyManager : IEconomyManager
    {
        private int _money;

        public EconomyManager(int initialMoney)
        {
            _money = initialMoney;
        }
        
        public void AddMoney(int amount)
        {
            _money += amount;
            
            EventBus.Publish(new EconomyEvent(EconomyEventType.MoneyAdded, amount, _money));
        }

        public void RemoveMoney(int amount)
        {
            if (_money >= amount)
            {
                _money -= amount;
                EventBus.Publish(new EconomyEvent(EconomyEventType.MoneyRemoved, amount, _money));
            }
        }

        public int GetMoney()
        {
            return _money;
        }

        public void SetMoney(int amount)
        {
            if (amount >= 0)
            {
                _money = amount;
            }
        }
    }
}