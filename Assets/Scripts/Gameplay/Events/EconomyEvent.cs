namespace Gameplay.Events
{
    public enum EconomyEventType
    {
        MoneyAdded,
        MoneyRemoved
    }
    
    public class EconomyEvent : IBusEvent
    {
        public EconomyEventType EventType { get; }
        public int Amount { get; }
        public int CurrentMoney { get; set; }

        public EconomyEvent(EconomyEventType eventType, int amount, int currentMoney)
        {
            EventType = eventType;
            Amount = amount;
            CurrentMoney = currentMoney;
        }
    }
}