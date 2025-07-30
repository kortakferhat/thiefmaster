using UnityEngine;

namespace Gameplay.Graph
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "ThiefMaster/Grid Config")]
    public class GridConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        public float gridSpacing = 2f;
        public float nodeYPosition = 0f;
        
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