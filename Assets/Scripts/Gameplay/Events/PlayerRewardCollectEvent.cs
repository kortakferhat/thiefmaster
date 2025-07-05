namespace Gameplay.Events
{
    public struct PlayerRewardCollectEvent : IBusEvent
    {
        public int Amount { get; }

        public PlayerRewardCollectEvent(int amount)
        {
            Amount = amount;
        }
    }
}