namespace Infrastructure.Managers.PoolManager
{
    public static class PoolKeys
    {
        // Entities
        public static string Bullet = "Bullet";
        public static string BouncingBullet = "BouncingBullet";
        public static string PiercingBullet = "PiercingBullet";
        public static string ExplosiveBouncingBullet = "ExplosiveBouncingBullet";
        public static string HomingBullet = "HomingBullet";
        public static string Wave = "Wave";
        public static string Orb = "Orb";
        public static string Enemy = "Enemy";
        public static string Coin = "Coin";
        
        // TowerFloors
        public static string TowerFloorPrefix = "TowerFloor";
        
        public static string TowerFloorTurret = $"{TowerFloorPrefix}Turret";
        public static string TowerFloorBomb = $"{TowerFloorPrefix}Bomb";
        public static string TowerFloorOrb = $"{TowerFloorPrefix}Orb";
        public static string TowerFloorWave = $"{TowerFloorPrefix}Wave";
        public static string TowerFloorMirror = $"{TowerFloorPrefix}Mirror";
        public static string TowerFloorBouncingTurret = $"{TowerFloorPrefix}Bouncing";
        public static string TowerFloorPiercingTurret = $"{TowerFloorPrefix}Piercing";
        public static string TowerFloorExplosiveBouncingTurret = $"{TowerFloorPrefix}ExplosiveBouncing";
        public static string TowerFloorHomingTurret = $"{TowerFloorPrefix}Homing";
        public static string TowerFloorHybridTurret = $"{TowerFloorPrefix}Hybrid";
        
        // VFX
        public static string HitVFX = "HitVFX";
        public static string TowerExplosionVFX = "TowerExplosion";
        public static string EnemyExplosion = "EnemyExplosion";
        
        // UI
        public static string TowerAddFloorButton = "TowerAddFloorButton";
        public static string Tooltip = "Tooltip";
        
    }
}