using UnityEngine;

namespace Gameplay.Graph
{
    public class NodeGizmo : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [SerializeField] private Vector2Int nodeId;
        [SerializeField] private NodeType nodeType;
        [SerializeField] private Color gizmoColor = Color.white;
        [SerializeField] private float labelOffset = 1f;
        
        public void Initialize(Vector2Int id, NodeType type)
        {
            nodeId = id;
            nodeType = type;
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            // Draw the node ID as text
            Vector3 labelPosition = transform.position + Vector3.up * labelOffset;
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = gizmoColor;
            UnityEditor.Handles.Label(labelPosition, $"({nodeId.x},{nodeId.y})");
            #endif
        }
    }
} 