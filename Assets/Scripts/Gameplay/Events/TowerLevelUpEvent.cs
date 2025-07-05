namespace Gameplay.Events
{
    public struct TowerLevelUpEvent : IBusEvent
    {
        public int Level { get; }
        public int Experience { get; }

        public TowerLevelUpEvent(int level, int experience)
        {
            Level = level;
            Experience = experience;
        }
    }
}