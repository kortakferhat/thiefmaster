using UnityEngine;

namespace Gameplay.Configs
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "TowerClicker/Character Config", order = 3)]
    public class CharacterConfigSO : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
    }
} 