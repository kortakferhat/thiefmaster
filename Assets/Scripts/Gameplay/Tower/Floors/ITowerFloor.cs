namespace Gameplay.Floors
{
    public interface ITowerFloor
    {
        public TowerFloorData FloorData { get; }
        void Initialize();
        void Upgrade();
        void Downgrade();
    }
}