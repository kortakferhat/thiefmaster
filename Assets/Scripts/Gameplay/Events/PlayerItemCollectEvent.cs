using Gameplay.Collectables;

namespace Gameplay.Events
{
    public class PlayerItemCollectEvent : IBusEvent
    {
        public ItemType ItemType { get; }
        
        public PlayerItemCollectEvent(ItemType itemType)
        {
            ItemType = itemType;
        }
    }
}