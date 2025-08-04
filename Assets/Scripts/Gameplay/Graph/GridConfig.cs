using UnityEngine;

namespace Gameplay.Graph
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "ThiefMaster/Grid Config")]
    public class GridConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        public float gridSpacing = 2f;
        public float nodeYPosition = 0f;
        
        [Header("Screen Positioning")]
        [Range(0f, 1f)]
        [Tooltip("Vertical position as percentage of screen height (0 = bottom, 1 = top)")]
        public float verticalOffsetPercentage = 0.5f;
        
        [Tooltip("Additional vertical offset in world units")]
        public float additionalVerticalOffset = 0f;
        
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