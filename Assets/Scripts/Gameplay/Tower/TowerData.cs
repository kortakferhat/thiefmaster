namespace Gameplay.MVP.Tower
{
    public class TowerData
    {
        /// <summary>
        /// Current tower level
        /// </summary>
        public int Level { get; set; } = 1;
        
        /// <summary>
        /// Current experience points
        /// </summary>
        public int Experience { get; set; } = 0;
        
        /// <summary>
        /// Maximum amount of floors the tower can have
        /// </summary>
        public int MaxFloors { get; set; } = 5;
        
        /// <summary>
        /// Resets tower data to default values
        /// </summary>
        public void Reset()
        {
            Level = 1;
            Experience = 0;
        }
    }
}