using UnityEngine;

namespace Gameplay.Configs
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "TowerClicker/Character Config", order = 3)]
    public class CharacterConfigSO : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float inputSensitivity = 1f;
        
        [Header("Character Properties")]
        [SerializeField] private float characterHeight = 1.8f;
        [SerializeField] private float characterRadius = 0.5f;
        
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float InputSensitivity => inputSensitivity;
        public float CharacterHeight => characterHeight;
        public float CharacterRadius => characterRadius;
    }
} 