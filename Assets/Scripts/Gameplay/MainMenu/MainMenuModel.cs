using Gameplay.MVP;

namespace Gameplay.MainMenu
{
    public class MainMenuModel : IModel
    {
        public int Money { get; private set; } = 0;

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
    }
}