namespace Infrastructure.Managers.EconomyManager
{
    public interface IEconomyManager : IService
    {
        void AddMoney(int amount);
        void RemoveMoney(int amount);
        int GetMoney();
        void SetMoney(int amount);
    }
}