using UnityEngine;

namespace Gameplay.Enemy
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "TowerClicker/Enemy Config", order = 2)]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("Enemy")] 
        public string enemyName;
        public float speed = 5f;
        public int damage = 1;
        public int health = 1;
        public float attackSpeed = 1;
    }
}