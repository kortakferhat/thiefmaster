using UnityEngine;

namespace Gameplay.Graph
{
    public enum LevelRotation
    {
        Right = 0,  
        Down = 90,  
        Left = 180, 
        Up = 270    
    }

    [CreateAssetMenu(fileName = "GridConfig", menuName = "ThiefMaster/Grid Config")]
    public class GridConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        public float gridSpacing = 2f;
        public float nodeYPosition = 0f;
        
        [Header("Level Rotation")]
        public LevelRotation levelRotation = LevelRotation.Right;
        
        [Header("Edge Settings")]
        public float edgeLength = 2f;
        public float edgeWidth = 0.1f;
        public float edgeYOffset = 0.1f;
        
        [Header("Visual Settings")]
        public bool showGrid = true;
        public bool showEdges = true;
        public bool showNodes = true;
    }
} 