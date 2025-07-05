# Advanced Bullet System Documentation

## Overview

Bu döküman, Tower Clicker oyununda implement edilen gelişmiş bullet sistemini açıklar. Sistem object-oriented prensiplerine uygun olarak tasarlanmış ve farklı bullet türlerini destekler.

## Architecture

### BaseBullet Class
Tüm bullet türleri için base class. Ortak functionality'leri sağlar:
- Temel hareket ve collision detection
- Pool management integration
- Damage dealing ve VFX handling
- Extensible hook points

### EnemyUtils Integration
Tüm bullet sistemleri `EnemyUtils.GetNearestEnemy()` methodunu kullanarak en yakın düşmanı bulur. Bu consistent ve optimized bir yaklaşım sağlar.

### Bullet Types

#### 1. Standard Bullet (`Bullet.cs`)
- **Davranış**: Düşmana çarpınca yok olur
- **Kullanım**: Temel savunma için
- **Pool Key**: `PoolKeys.Bullet`
- **Tower Floor**: `TowerFloorTurret`
- **Weapon**: `TurretWeapon`

#### 2. Bouncing Bullet (`BouncingBullet.cs`)
- **Davranış**: Düşmandan düşmana maksimum `maxBounces` kadar seker
- **Özellikler**:
  - `maxBounces`: Maksimum sekme sayısı (default: 3)
  - `bounceRange`: Sekme mesafesi (default: 5f)
  - `bounceSpeedMultiplier`: Her sekmede hız artışı (default: 1.2f)
- **Pool Key**: `PoolKeys.BouncingBullet`
- **Tower Floor**: `TowerFloorBouncingTurret`
- **Weapon**: `BouncingTurretWeapon`

#### 3. Piercing Bullet (`PiercingBullet.cs`)
- **Davranış**: Düşmanların içinden geçer, maksimum `maxPierces` kadar vuruş yapabilir
- **Özellikler**:
  - `maxPierces`: Maksimum delme sayısı (default: 5)
  - `damageReductionPerPierce`: Her delmede hasar azalması (default: 0.1f = %10)
- **Pool Key**: `PoolKeys.PiercingBullet`
- **Tower Floor**: `TowerFloorPiercingTurret`
- **Weapon**: `PiercingTurretWeapon`

#### 4. Explosive Bouncing Bullet (`ExplosiveBouncingBullet.cs`)
- **Davranış**: Hem seker hem de her vuruşta area damage yapar
- **Özellikler**:
  - Bouncing bullet özellikleri
  - `explosionRadius`: Patlama yarıçapı (default: 3f)
  - `explosionDamage`: Patlama hasarı (default: 2)
  - Distance-based damage falloff
- **Pool Key**: `PoolKeys.ExplosiveBouncingBullet`
- **Tower Floor**: `TowerFloorExplosiveBouncingTurret`
- **Weapon**: `ExplosiveBouncingTurretWeapon`

#### 5. Homing Bullet (`HomingBullet.cs`)
- **Davranış**: Hedefi aktif olarak takip eder
- **Özellikler**:
  - `homingStrength`: Takip kuvveti (default: 2f)
  - `maxLifetime`: Maksimum yaşam süresi (default: 8f)
  - `retargetInterval`: Hedef değiştirme aralığı (default: 0.5f)
  - `targetingRange`: Hedefleme mesafesi (default: 15f)
- **Pool Key**: `PoolKeys.HomingBullet`
- **Tower Floor**: `TowerFloorHomingTurret`
- **Weapon**: `HomingTurretWeapon`

#### 6. Hybrid Turret (`HybridTurretWeapon.cs`)
- **Davranış**: Farklı bullet türlerini probability-based olarak seçer
- **Özellikler**:
  - %40 Normal Bullet
  - %30 Bouncing Bullet
  - %20 Piercing Bullet
  - %10 Explosive Bouncing Bullet
- **Tower Floor**: `TowerFloorHybridTurret`
- **Weapon**: `HybridTurretWeapon`

## Tower Floor Integration

### Tower Floor Types
`TowerFloorType` enum'una eklenen yeni türler:
- `BouncingTurret`
- `PiercingTurret`
- `ExplosiveBouncingTurret`
- `HomingTurret`
- `HybridTurret`

### Pool Keys
Her tower floor için karşılık gelen pool key'ler:
- `PoolKeys.TowerFloorBouncingTurret`
- `PoolKeys.TowerFloorPiercingTurret`
- `PoolKeys.TowerFloorExplosiveBouncingTurret`
- `PoolKeys.TowerFloorHomingTurret`
- `PoolKeys.TowerFloorHybridTurret`

### Tower Floor Classes
Her bullet tipi için özel tower floor class'ları:
```csharp
// Example usage in TowerFloorManager
towerManager.AddToTop(PoolKeys.TowerFloorBouncingTurret);
towerManager.AddToTop(PoolKeys.TowerFloorPiercingTurret);
```

## Weapon Integration

### Fire Rate Balance
- **Standard Turret**: 0.5s (2 bullets/second)
- **Bouncing Turret**: 0.4s (2.5 bullets/second)
- **Piercing Turret**: 0.3s (3.33 bullets/second)
- **Explosive Bouncing Turret**: 0.25s (4 bullets/second) - Lowest due to high power
- **Homing Turret**: 0.6s (1.67 bullets/second) - Higher rate due to guaranteed hits
- **Hybrid Turret**: 0.35s (2.86 bullets/second)

### Example Weapon Usage

#### BouncingTurretWeapon
```csharp
var bullet = poolManager.Spawn(PoolKeys.BouncingBullet, firePoint.position, Quaternion.identity)
    .GetComponent<BouncingBullet>();
bullet.Fire(nearestEnemy.transform.position, PoolKeys.BouncingBullet);
```

#### EnemyUtils Integration
```csharp
var nearestEnemy = EnemyUtils.GetNearestEnemy(firePoint.position);
if (nearestEnemy == null) return false;
```

## Extending the System

### Yeni Bullet Türü Ekleme

1. **Bullet Class**: `BaseBullet`'dan inherit edin
2. **Pool Key**: `PoolKeys.cs`'ye ekleyin
3. **Weapon Class**: `BaseWeapon`'dan inherit edin
4. **Tower Floor Class**: `TowerFloor`'dan inherit edin
5. **Tower Floor Type**: Enum'a ekleyin
6. **Prefab'lar**: Unity'de oluşturun

### Example Implementation
```csharp
// 1. Bullet Class
public class FreezeBullet : BaseBullet
{
    protected override string GetDefaultPoolKey() => PoolKeys.FreezeBullet;
    protected override void OnEnemyHit(Collider enemyCollider, Enemy enemy)
    {
        // Apply freeze effect
        enemy.ApplySlowEffect(freezeDuration, slowMultiplier);
        Despawn();
    }
}

// 2. Weapon Class
public class FreezeTurretWeapon : BaseWeapon
{
    bool Fire()
    {
        var nearestEnemy = EnemyUtils.GetNearestEnemy(firePoint.position);
        if (nearestEnemy == null) return false;
        
        var bullet = poolManager.Spawn(PoolKeys.FreezeBullet, firePoint.position, Quaternion.identity)
            .GetComponent<FreezeBullet>();
        bullet.Fire(nearestEnemy.transform.position, PoolKeys.FreezeBullet);
        return true;
    }
}

// 3. Tower Floor Class
public class TowerFloorFreezeTurret : TowerFloor { }
```

## Performance Considerations

- **Object Pooling**: Tüm bullet'lar pool'dan spawn edilir
- **EnemyUtils**: Optimized enemy finding algoritması
- **Memory Management**: HashSet ve List kullanımı efficient
- **Collision Detection**: Sadece "Enemy" tag'li objeler için
- **VFX Management**: Particle system'ler pool'dan yönetilir

## Usage Tips

1. **Tower Placement Strategy**: Farklı bullet türlerini stratejik olarak yerleştirin
2. **Cost vs Effectiveness**: Powerful bullet'lar daha pahalı olmalı
3. **Visual Feedback**: Her bullet türü için unique trail/particle effect'ler
4. **Balanced Gameplay**: Fire rate ve damage balance'ı oyun zorluk seviyesine göre ayarlayın

## Future Enhancements

- **Chain Lightning Bullet**: Düşmanlar arası elektrik zinciri
- **Freeze Bullet**: Düşmanları yavaşlatan/donduran bullet
- **Magnetic Bullet**: Yakındaki coin'leri çeken bullet
- **Splitting Bullet**: Çarpışmada ikiye bölünen bullet
- **Poison Bullet**: DoT (Damage over Time) effect'li bullet
- **Shield Piercing Bullet**: Özel savunmaları delen bullet

## Integration Checklist

- ✅ BaseBullet inheritance system
- ✅ EnemyUtils integration
- ✅ Pool management
- ✅ Tower floor integration
- ✅ Weapon system integration
- ✅ VFX and particle effects
- ✅ Performance optimization
- ✅ Extensible architecture
- ✅ Complete documentation

Bu sistem sayesinde oyuna kolayca yeni bullet türleri ekleyebilir ve mevcut sistemle seamless entegrasyon sağlayabilirsiniz. 