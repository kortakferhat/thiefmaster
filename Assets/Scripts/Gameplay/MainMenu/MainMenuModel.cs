using System.Collections.Generic;
using Gameplay.Collectables;
using Gameplay.MVP;

namespace Gameplay.MainMenu
{
    public class MainMenuModel : IModel
    {
        public int Money { get; private set; } = 0;
        public int RemainingMoves { get; private set; } = 0;
        public Dictionary<ItemType, int> Items { get; private set; } = new Dictionary<ItemType, int>();

        public void SetRemainingMoves(int amount)
        {
            RemainingMoves = amount;
        }
        
        public void SetMoney(int money)
        {
            Money = money;
        }

        public void AddMoney(int amount)
        {
            Money += amount;
        }

        public void Reset()
        {
            Money = 0;
        }

        public void AddFloorToTop(string poolType)
        {
            
        }

        public void AddItem(ItemType argsItemType)
        {
            if (!Items.TryAdd(argsItemType, 1))
            {
                Items[argsItemType]++;
            }
        }
    }
}