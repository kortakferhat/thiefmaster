using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Gameplay.Graph;
using Gameplay.Enemy;

namespace Editor
{
    /// <summary>
    /// Graph Editor Window - Visual level editor for creating graph-based levels
    /// 
    /// This editor works with EdgeData for serialization, but at runtime the graph
    /// is converted to a LinkedList/Adjacency structure for O(1) performance.
    /// 
    /// Editor Performance: O(n) lookup is acceptable since it's only used for level editing
    /// Runtime Performance: O(1) lookup via NodeConnection system for gameplay
    /// </summary>
    public class GraphEditorWindow : EditorWindow
    {
        private GraphScriptableObject currentGraph;
        private Vector2 scrollPosition;
        private Vector2 panOffset = Vector2.zero;
        private float zoom = 1f;
        private bool isDragging = false;
        private Vector2 lastMousePosition;
        private NodeData selectedNode;
        private EdgeData selectedEdge;
        private bool isCreatingEdge = false;
        private NodeData edgeStartNode;
        private Vector2 mousePosition;
        private bool showGrid = true;
        private float gridSize = 50f;
        private bool showHelpWindow = false;
        private bool isDraggingNode = false;
        private NodeData draggedNode;
        private Vector2 dragOffset;
        private bool edgeCreationMode = false;
        private NodeData lastSelectedNode;
        private EdgeData lastSelectedEdge;
        private EditorMode currentMode = EditorMode.Select;
        private string modeFeedback = "";
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();

        public interface ICommand
        {
            void Execute();
            void Undo();
        }

        public class AddNodeCommand : ICommand
        {
            private GraphData graphData;
            private NodeData node;
            private bool wasExecuted = false;

            public AddNodeCommand(GraphData graphData, NodeData node)
            {
                this.graphData = graphData;
                this.node = node;
            }

            public void Execute()
            {
                if (!wasExecuted)
                {
                    graphData.nodes.Add(node);
                    wasExecuted = true;
                }
            }

            public void Undo()
            {
                if (wasExecuted)
                {
                    graphData.nodes.Remove(node);
                    // Remove connected edges
                    graphData.edges.RemoveAll(e => e.fromId == node.id || e.toId == node.id);
                }
            }
        }

        public class DeleteNodeCommand : ICommand
        {
            private GraphData graphData;
            private NodeData node;
            private List<EdgeData> connectedEdges;
            private bool wasExecuted = false;

            public DeleteNodeCommand(GraphData graphData, NodeData node)
            {
                this.graphData = graphData;
                this.node = node;
                this.connectedEdges = new List<EdgeData>();
            }

            public void Execute()
            {
                if (!wasExecuted)
                {
                    // Store connected edges before deletion
                    connectedEdges = graphData.edges.Where(e => e.fromId == node.id || e.toId == node.id).ToList();
                    graphData.nodes.Remove(node);
                    graphData.edges.RemoveAll(e => e.fromId == node.id || e.toId == node.id);
                    wasExecuted = true;
                }
            }

            public void Undo()
            {
                if (wasExecuted)
                {
                    graphData.nodes.Add(node);
                    graphData.edges.AddRange(connectedEdges);
                }
            }
        }

        public class AddEdgeCommand : ICommand
        {
            private GraphData graphData;
            private EdgeData edge;
            private bool wasExecuted = false;

            public AddEdgeCommand(GraphData graphData, EdgeData edge)
            {
                this.graphData = graphData;
                this.edge = edge;
            }

            public void Execute()
            {
                if (!wasExecuted)
                {
                    graphData.edges.Add(edge);
                    wasExecuted = true;
                }
            }

            public void Undo()
            {
                if (wasExecuted)
                {
                    graphData.edges.Remove(edge);
                }
            }
        }

        public class DeleteEdgeCommand : ICommand
        {
            private GraphData graphData;
            private EdgeData edge;
            private bool wasExecuted = false;

            public DeleteEdgeCommand(GraphData graphData, EdgeData edge)
            {
                this.graphData = graphData;
                this.edge = edge;
            }

            public void Execute()
            {
                if (!wasExecuted)
                {
                    graphData.edges.Remove(edge);
                    wasExecuted = true;
                }
            }

            public void Undo()
            {
                if (wasExecuted)
                {
                    graphData.edges.Add(edge);
                }
            }
        }

        public class MoveNodeCommand : ICommand
        {
            private NodeData node;
            private Vector2Int oldPosition;
            private Vector2Int newPosition;
            private bool wasExecuted = false;

            public MoveNodeCommand(NodeData node, Vector2Int newPosition)
            {
                this.node = node;
                this.oldPosition = node.id;
                this.newPosition = newPosition;
            }

            public void Execute()
            {
                if (!wasExecuted)
                {
                    node.id = newPosition;
                    wasExecuted = true;
                }
            }

            public void Undo()
            {
                if (wasExecuted)
                {
                    node.id = oldPosition;
                }
            }
        }

        public enum EditorMode
        {
            Select,
            CreateNode,
            CreateEdge,
            Delete,
            Move
        }

        // Colors for different node types
        private readonly Dictionary<NodeType, Color> nodeColors = new()
        {
            { NodeType.Normal, Color.white },
            { NodeType.Start, Color.green },
            { NodeType.Goal, Color.red },
            { NodeType.Breakable, Color.yellow },
            { NodeType.Redirector, Color.blue },
            { NodeType.Trap, Color.black },
            { NodeType.Enemy, Color.magenta }
        };

        // Colors for different edge types
        private readonly Dictionary<EdgeType, Color> edgeColors = new()
        {
            { EdgeType.Standard, Color.white },
            { EdgeType.Directed, Color.cyan },
            { EdgeType.Slippery, Color.orange },
            { EdgeType.Breakable, Color.red }
        };

        [MenuItem("Tools/Graph Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<GraphEditorWindow>("Graph Editor");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            // Initialize pan offset on first GUI call when window is properly sized
            if (panOffset == Vector2.zero && position.width > 100)
            {
                ResetView();
                // Create a new graph with default nodes if none is loaded
                if (currentGraph == null)
                {
                    CreateNewGraph();
                }
            }
            
            DrawToolbar();
            EditorGUILayout.BeginHorizontal();
            DrawGraphArea();
            DrawInspector();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Graph selection
            EditorGUI.BeginChangeCheck();
            currentGraph = (GraphScriptableObject)EditorGUILayout.ObjectField(currentGraph, typeof(GraphScriptableObject), false, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck() && currentGraph != null)
            {
                selectedNode = null;
                selectedEdge = null;
                isCreatingEdge = false;
                CheckSelectionChange();
            }

            GUILayout.FlexibleSpace();

            // New graph button
            if (GUILayout.Button("New Graph", EditorStyles.toolbarButton))
            {
                CreateNewGraph();
            }

            // Save button
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                SaveGraph();
            }

            // Undo/Redo buttons
            GUI.enabled = undoStack.Count > 0;
            if (GUILayout.Button("Undo", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                Undo();
            }
            GUI.enabled = redoStack.Count > 0;
            if (GUILayout.Button("Redo", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                Redo();
            }
            GUI.enabled = true;

            // Clear button
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Clear Graph", "Are you sure you want to clear the graph?", "Yes", "No"))
                {
                    currentGraph?.ClearGraph();
                    selectedNode = null;
                    selectedEdge = null;
                    isCreatingEdge = false;
                }
            }

            // Grid toggle
            showGrid = GUILayout.Toggle(showGrid, "Grid", EditorStyles.toolbarButton);
            
            // Mode selection
            GUILayout.Space(10);
            GUILayout.Label("Mode:", EditorStyles.toolbarButton, GUILayout.Width(40));
            
            // Create custom button styles for each mode
            var selectStyle = new GUIStyle(EditorStyles.toolbarButton);
            var nodeStyle = new GUIStyle(EditorStyles.toolbarButton);
            var edgeStyle = new GUIStyle(EditorStyles.toolbarButton);
            var deleteStyle = new GUIStyle(EditorStyles.toolbarButton);
            var moveStyle = new GUIStyle(EditorStyles.toolbarButton);
            
            // Set active state colors
            if (currentMode == EditorMode.Select)
            {
                selectStyle.normal.textColor = Color.white;
                selectStyle.active.textColor = Color.white;
                selectStyle.normal.background = EditorGUIUtility.whiteTexture;
            }
            else
            {
                selectStyle.normal.textColor = Color.gray;
            }
            
            if (currentMode == EditorMode.CreateNode)
            {
                nodeStyle.normal.textColor = Color.white;
                nodeStyle.active.textColor = Color.white;
                nodeStyle.normal.background = EditorGUIUtility.whiteTexture;
            }
            else
            {
                nodeStyle.normal.textColor = Color.gray;
            }
            
            if (currentMode == EditorMode.CreateEdge)
            {
                edgeStyle.normal.textColor = Color.white;
                edgeStyle.active.textColor = Color.white;
                edgeStyle.normal.background = EditorGUIUtility.whiteTexture;
            }
            else
            {
                edgeStyle.normal.textColor = Color.gray;
            }
            
            if (currentMode == EditorMode.Delete)
            {
                deleteStyle.normal.textColor = Color.white;
                deleteStyle.active.textColor = Color.white;
                deleteStyle.normal.background = EditorGUIUtility.whiteTexture;
            }
            else
            {
                deleteStyle.normal.textColor = Color.gray;
            }
            
            if (currentMode == EditorMode.Move)
            {
                moveStyle.normal.textColor = Color.white;
                moveStyle.active.textColor = Color.white;
                moveStyle.normal.background = EditorGUIUtility.whiteTexture;
            }
            else
            {
                moveStyle.normal.textColor = Color.gray;
            }
            
            if (GUILayout.Button("Select", selectStyle, GUILayout.Width(50)))
            {
                currentMode = EditorMode.Select;
                modeFeedback = "Select Mode: Click to select nodes/edges";
            }
            
            if (GUILayout.Button("Node", nodeStyle, GUILayout.Width(50)))
            {
                currentMode = EditorMode.CreateNode;
                modeFeedback = "Node Mode: Click to create nodes";
            }
            
            if (GUILayout.Button("Edge", edgeStyle, GUILayout.Width(50)))
            {
                currentMode = EditorMode.CreateEdge;
                modeFeedback = "Edge Mode: Click nodes to create edges";
            }
            
            if (GUILayout.Button("Delete", deleteStyle, GUILayout.Width(50)))
            {
                currentMode = EditorMode.Delete;
                modeFeedback = "Delete Mode: Click to delete nodes/edges";
            }
            
            if (GUILayout.Button("Move", moveStyle, GUILayout.Width(50)))
            {
                currentMode = EditorMode.Move;
                modeFeedback = "Move Mode: Drag nodes to move them";
            }
            
            // Edge creation mode toggle (legacy)
            edgeCreationMode = GUILayout.Toggle(edgeCreationMode, "Edge Mode", EditorStyles.toolbarButton);
            
            // Zoom controls
            GUILayout.Space(10);
            GUILayout.Label("Zoom:", EditorStyles.toolbarButton, GUILayout.Width(40));
            
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                var oldZoom = zoom;
                zoom = Mathf.Clamp(zoom + 0.1f, 0.1f, 5f); // Reduced from 0.2f
                
                // Zoom towards graph area center (0,0)
                var graphAreaWidth = position.width - 300; // Inspector width is 300
                var center = new Vector2(graphAreaWidth / 2, position.height / 2);
                var centerWorldPos = (center - panOffset) / oldZoom;
                panOffset = center - centerWorldPos * zoom;
                
                // Clear GUI focus to ensure selection works after zoom
                GUI.FocusControl(null);
                Repaint();
            }
            
            GUILayout.Label($"{zoom:F1}x", EditorStyles.toolbarButton, GUILayout.Width(40));
            
            // Debug info
            GUILayout.Label($"Pan: {panOffset.x:F0},{panOffset.y:F0}", EditorStyles.toolbarButton, GUILayout.Width(80));
            
            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                var oldZoom = zoom;
                zoom = Mathf.Clamp(zoom - 0.1f, 0.1f, 5f); // Reduced from 0.2f
                
                // Zoom towards graph area center (0,0)
                var graphAreaWidth = position.width - 300; // Inspector width is 300
                var center = new Vector2(graphAreaWidth / 2, position.height / 2);
                var centerWorldPos = (center - panOffset) / oldZoom;
                panOffset = center - centerWorldPos * zoom;
                
                // Clear GUI focus to ensure selection works after zoom
                GUI.FocusControl(null);
                Repaint();
            }
            
            if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                ResetView();
                // Clear GUI focus to ensure selection works after reset
                GUI.FocusControl(null);
                Repaint();
            }
            
            // Help button
            if (GUILayout.Button("?", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                showHelpWindow = !showHelpWindow;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGraphArea()
        {
            // Main graph area
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            var graphRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            // Handle input
            HandleInput(graphRect);
            
            // Draw graph
            DrawGraph(graphRect);
            
            // Draw help window if enabled
            if (showHelpWindow)
            {
                DrawHelpWindow();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void HandleInput(Rect graphRect)
        {
            var e = Event.current;
            mousePosition = e.mousePosition;

            // Transform mouse position to account for GUI.matrix transformation
            var transformedMousePos = TransformMousePosition(e.mousePosition);

            // Handle input even if mouse is outside graph area for zoom and pan
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (!graphRect.Contains(e.mousePosition))
                        return;

                    if (e.button == 0) // Left click
                    {
                        switch (currentMode)
                        {
                            case EditorMode.Select:
                                SelectAtPosition(transformedMousePos);
                                if (selectedNode != null)
                                {
                                    StartNodeDrag(transformedMousePos);
                                }
                                else
                                {
                                    isDragging = true;
                                    lastMousePosition = e.mousePosition;
                                    lastSelectedNode = selectedNode;
                                    lastSelectedEdge = selectedEdge;
                                }
                                break;
                                
                            case EditorMode.CreateNode:
                                CreateNodeAtPosition(transformedMousePos);
                                modeFeedback = $"Node created at {GetNodeIdFromScreenPosition(transformedMousePos)}";
                                break;
                                
                            case EditorMode.CreateEdge:
                                HandleEdgeModeClick(transformedMousePos);
                                break;
                                
                            case EditorMode.Delete:
                                var nodeToDelete = FindNodeAtPosition(transformedMousePos);
                                var edgeToDelete = FindEdgeAtPosition(transformedMousePos);
                                
                                if (nodeToDelete != null)
                                {
                                    DeleteNode(nodeToDelete);
                                    modeFeedback = $"Node deleted at {nodeToDelete.id}";
                                }
                                else if (edgeToDelete != null)
                                {
                                    var command = new DeleteEdgeCommand(currentGraph.graphData, edgeToDelete);
                                    ExecuteCommand(command);
                                    modeFeedback = $"Edge deleted from {edgeToDelete.fromId} to {edgeToDelete.toId}";
                                }
                                break;
                                
                            case EditorMode.Move:
                                var nodeToMove = FindNodeAtPosition(transformedMousePos);
                                if (nodeToMove != null)
                                {
                                    selectedNode = nodeToMove;
                                    StartNodeDrag(transformedMousePos);
                                    modeFeedback = $"Moving node at {nodeToMove.id}";
                                }
                                break;
                        }
                        e.Use();
                    }
                    else if (e.button == 1) // Right click
                    {
                        ShowContextMenu(transformedMousePos);
                        e.Use();
                    }
                    else if (e.button == 2) // Middle click for panning
                    {
                        isDragging = true;
                        lastMousePosition = e.mousePosition;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (isDragging && (e.button == 2 || e.button == 0))
                    {
                        // Pan the view (middle click or left click on empty space)
                        panOffset += (e.mousePosition - lastMousePosition) / zoom;
                        lastMousePosition = e.mousePosition;
                        Repaint();
                        // Don't use e.Use() for panning to avoid breaking event system
                    }
                    else if (isDraggingNode && draggedNode != null)
                    {
                        UpdateNodeDrag(TransformMousePosition(e.mousePosition));
                        Repaint();
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (e.button == 2 || (e.button == 0 && isDragging && !isDraggingNode))
                    {
                        isDragging = false;
                        // Restore selection after panning
                        selectedNode = lastSelectedNode;
                        selectedEdge = lastSelectedEdge;
                        // Force a repaint to ensure proper state
                        Repaint();
                        // Don't use e.Use() for panning to avoid breaking event system
                    }
                    else if (e.button == 0 && isDraggingNode)
                    {
                        EndNodeDrag();
                        e.Use();
                    }
                    break;

                case EventType.ScrollWheel:
                    // Allow zoom even when mouse is outside graph area
                    var oldZoom = zoom;
                    var zoomSpeed = 0.05f; // Reduced sensitivity
                    
                    // Faster zoom when Ctrl is held
                    if (e.control)
                        zoomSpeed = 0.1f; // Reduced from 0.2f
                    
                    zoom = Mathf.Clamp(zoom - e.delta.y * zoomSpeed, 0.1f, 5f);
                    
                    // Zoom towards mouse position
                    var mouseWorldPos = (e.mousePosition - panOffset) / oldZoom;
                    panOffset = e.mousePosition - mouseWorldPos * zoom;
                    
                    Repaint();
                    e.Use();
                    break;

                case EventType.KeyDown:
                    // Keyboard shortcuts for zoom and pan
                    switch (e.keyCode)
                    {
                        case KeyCode.Z:
                            if (e.control)
                            {
                                if (e.shift)
                                {
                                    Redo();
                                }
                                else
                                {
                                    Undo();
                                }
                                e.Use();
                            }
                            break;
                            
                        case KeyCode.Y:
                            if (e.control)
                            {
                                Redo();
                                e.Use();
                            }
                            break;
                            
                        case KeyCode.Equals: // Plus key
                        case KeyCode.KeypadPlus:
                            if (e.control)
                            {
                                var oldZoom2 = zoom;
                                zoom = Mathf.Clamp(zoom + 0.1f, 0.1f, 5f);
                                
                                // Zoom towards graph area center (0,0)
                                var graphAreaWidth1 = position.width - 300; // Inspector width is 300
                                var centerPos = new Vector2(graphAreaWidth1 / 2, position.height / 2);
                                var centerWorldPos = (centerPos - panOffset) / oldZoom2;
                                panOffset = centerPos - centerWorldPos * zoom;
                                
                                // Clear GUI focus to ensure selection works after zoom
                                GUI.FocusControl(null);
                                Repaint();
                                e.Use();
                            }
                            break;
                            
                        case KeyCode.Minus: // Minus key
                        case KeyCode.KeypadMinus:
                            if (e.control)
                            {
                                var oldZoom2 = zoom;
                                zoom = Mathf.Clamp(zoom - 0.1f, 0.1f, 5f);
                                // Zoom towards graph area center (0,0)
                                var graphAreaWidth2 = position.width - 300; // Inspector width is 300
                                var centerPos = new Vector2(graphAreaWidth2 / 2, position.height / 2);
                                var centerWorldPos = (centerPos - panOffset) / oldZoom2;
                                panOffset = centerPos - centerWorldPos * zoom;
                                
                                // Clear GUI focus to ensure selection works after zoom
                                GUI.FocusControl(null);
                                Repaint();
                                e.Use();
                            }
                            break;
                            
                        case KeyCode.Home:
                            // Reset view - center (0,0) on graph area
                            ResetView();
                            // Clear GUI focus to ensure selection works after reset
                            GUI.FocusControl(null);
                            Repaint();
                            e.Use();
                            break;
                            
                        case KeyCode.Space:
                            // Toggle grid
                            showGrid = !showGrid;
                            Repaint();
                            e.Use();
                            break;
                    }
                    break;
            }
            
            // Handle touch input for mobile devices
            HandleTouchInput(graphRect);
        }

        private void ResetView()
        {
            float inspectorWidth = 300f;
            float graphAreaWidth = position.width - inspectorWidth;
            
            // Toolbar height (fallback to 20 if toolbar style not yet initialised)
            float toolbarHeight = EditorStyles.toolbar.fixedHeight;
            if (toolbarHeight <= 0f) toolbarHeight = 20f;
            float graphAreaHeight = position.height - toolbarHeight;
            
            // Center (0,0) in the middle of the graph area (ignoring inspector)
            panOffset = new Vector2(
                graphAreaWidth / 2f,
                toolbarHeight + graphAreaHeight / 2f
            );
            zoom = 1f;
        }

        private void HandleTouchInput(Rect graphRect)
        {
            // Handle touch input for mobile devices
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        // Single touch - treat as mouse click
                        if (graphRect.Contains(touch.position))
                        {
                            // Simulate mouse click
                            var mouseEvent = new Event
                            {
                                type = EventType.MouseDown,
                                button = 0,
                                mousePosition = touch.position
                            };
                            HandleInput(graphRect);
                        }
                        break;
                        
                    case TouchPhase.Moved:
                        // Single touch move - pan the view
                        if (Input.touchCount == 1)
                        {
                            panOffset += touch.deltaPosition / zoom;
                            Repaint();
                        }
                        // Two finger touch - zoom
                        else if (Input.touchCount == 2)
                        {
                            var touch0 = Input.GetTouch(0);
                            var touch1 = Input.GetTouch(1);
                            
                            // Calculate zoom based on distance between touches
                            var prevDistance = Vector2.Distance(touch0.position - touch0.deltaPosition, touch1.position - touch1.deltaPosition);
                            var currentDistance = Vector2.Distance(touch0.position, touch1.position);
                            
                            if (prevDistance > 0)
                            {
                                var zoomFactor = currentDistance / prevDistance;
                                var oldZoom = zoom;
                                zoom = Mathf.Clamp(zoom * zoomFactor, 0.1f, 5f);
                                
                                // Zoom towards center of touches
                                var center = (touch0.position + touch1.position) / 2f;
                                var centerWorldPos = (center - panOffset) / oldZoom;
                                panOffset = center - centerWorldPos * zoom;
                                
                                Repaint();
                            }
                        }
                        break;
                        
                    case TouchPhase.Ended:
                        // Touch ended
                        break;
                }
            }
        }

        private void DrawGraph(Rect graphRect)
        {
            if (currentGraph == null)
            {
                var style = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    normal = { textColor = Color.gray }
                };
                EditorGUI.LabelField(graphRect, "No graph selected. Create or load a graph to start editing.", style);
                return;
            }

            // Draw background
            DrawRect(graphRect, new Color(0.2f, 0.2f, 0.2f));

            // Apply transformations
            var matrix = Matrix4x4.TRS(panOffset, Quaternion.identity, Vector3.one * zoom);
            GUI.matrix = matrix;

            // Draw grid
            if (showGrid)
            {
                DrawGrid(graphRect);
            }

            // Draw edges first (behind nodes)
            DrawEdges(graphRect);

            // Draw nodes on top
            DrawNodes(graphRect);

            // Draw mode feedback
            if (!string.IsNullOrEmpty(modeFeedback))
            {
                DrawModeFeedback();
            }

            // Reset matrix
            GUI.matrix = Matrix4x4.identity;
        }

        private void DrawGrid(Rect graphRect)
        {
            var gridColor = new Color(0.3f, 0.3f, 0.5f, 0.5f);
            var gridWorldSize = gridSize;

            // Since GUI.matrix is already applied, we need to work in world coordinates directly
            // Calculate the visible world area based on the screen rect and current transform
            var matrix = Matrix4x4.TRS(panOffset, Quaternion.identity, Vector3.one * zoom);
            var inverseMatrix = matrix.inverse;
            
            var screenTopLeft = inverseMatrix.MultiplyPoint3x4(new Vector3(graphRect.xMin, graphRect.yMin, 0));
            var screenBottomRight = inverseMatrix.MultiplyPoint3x4(new Vector3(graphRect.xMax, graphRect.yMax, 0));

            var gridStartX = screenTopLeft.x;
            var gridStartY = screenTopLeft.y;
            var gridEndX = screenBottomRight.x;
            var gridEndY = screenBottomRight.y;

            // Align grid lines to the grid size
            gridStartX = Mathf.Floor(gridStartX / gridWorldSize) * gridWorldSize;
            gridStartY = Mathf.Floor(gridStartY / gridWorldSize) * gridWorldSize;
            gridEndX = Mathf.Ceil(gridEndX / gridWorldSize) * gridWorldSize;
            gridEndY = Mathf.Ceil(gridEndY / gridWorldSize) * gridWorldSize;

            // Draw vertical lines
            for (float x = gridStartX; x <= gridEndX; x += gridWorldSize)
            {
                DrawRect(new Rect(x, gridStartY, 1, gridEndY - gridStartY), gridColor);
            }

            // Draw horizontal lines
            for (float y = gridStartY; y <= gridEndY; y += gridWorldSize)
            {
                DrawRect(new Rect(gridStartX, y, gridEndX - gridStartX, 1), gridColor);
            }
        }



        private void DrawNodes(Rect graphRect)
        {
            if (currentGraph?.graphData?.nodes == null) return;

            foreach (var nodeData in currentGraph.graphData.nodes)
            {
                var nodePos = GetNodeScreenPosition(nodeData);
                var nodeSize = Mathf.Max(currentGraph.nodeSize, 20f / zoom); // Adjust minimum size for zoom
                var nodeRect = new Rect(nodePos.x - nodeSize / 2, nodePos.y - nodeSize / 2, nodeSize, nodeSize);

                // Draw node
                var nodeColor = nodeColors.GetValueOrDefault(nodeData.type, Color.white);
                if (nodeData == selectedNode)
                {
                    nodeColor = Color.Lerp(nodeColor, Color.yellow, 0.5f);
                }

                // Highlight dragged node
                if (nodeData == draggedNode && isDraggingNode)
                {
                    nodeColor = Color.Lerp(nodeColor, Color.cyan, 0.3f);
                }

                DrawCircle(nodePos, nodeSize / 2, nodeColor);
                DrawCircleBorder(nodePos, nodeSize / 2, Color.black, 2f);

                // Draw facing direction for enemy nodes
                if (nodeData.type == NodeType.Enemy)
                {
                    DrawEnemyFacingDirection(nodePos, nodeData.enemyFacingDirection, nodeSize);
                }

                // Draw node label with better positioning
                var labelRect = new Rect(nodePos.x - 30, nodePos.y + nodeSize / 2 + 5, 60, 20);
                var style = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Max(Mathf.RoundToInt(10), 8),
                    normal = { textColor = Color.white },
                    fontStyle = FontStyle.Bold
                };
                EditorGUI.LabelField(labelRect, nodeData.id.ToString(), style);
            }

            // Draw drag preview if dragging
            if (isDraggingNode && draggedNode != null)
            {
                DrawDragPreview();
            }
        }

        private void DrawEdges(Rect graphRect)
        {
            if (currentGraph?.graphData?.edges == null) return;

            // Create a dictionary for faster node lookup - O(1) instead of O(n) for each edge
            var nodeDict = currentGraph.graphData.nodes.ToDictionary(n => n.id);

            foreach (var edgeData in currentGraph.graphData.edges)
            {
                if (!nodeDict.TryGetValue(edgeData.fromId, out var fromNode) || 
                    !nodeDict.TryGetValue(edgeData.toId, out var toNode))
                    continue;
                
                var fromPos = GetNodeScreenPosition(fromNode);
                var toPos = GetNodeScreenPosition(toNode);

                var edgeColor = edgeColors.GetValueOrDefault(edgeData.type, Color.white);
                if (edgeData == selectedEdge)
                {
                    edgeColor = Color.Lerp(edgeColor, Color.yellow, 0.5f);
                    // Make selected edge thicker
                    var width = Mathf.Max(currentGraph.edgeWidth * 2f, 8f / zoom);
                    DrawLine(fromPos, toPos, edgeColor, width);
                }
                else
                {
                    var width = Mathf.Max(currentGraph.edgeWidth, 4f / zoom);
                    DrawLine(fromPos, toPos, edgeColor, width);
                }

                // Draw arrow for directed edges
                if (edgeData.type == EdgeType.Directed)
                {
                    var width = Mathf.Max(currentGraph.edgeWidth, 4f / zoom);
                    DrawArrow(fromPos, toPos, edgeColor, width);
                }
            }
        }

        private void DrawDragPreview()
        {
            if (draggedNode == null) return;

            // Calculate target grid position using transformed mouse position and drag offset
            var transformedMousePos = TransformMousePosition(mousePosition);
            var targetPos = transformedMousePos - dragOffset;
            var gridX = Mathf.RoundToInt(targetPos.x / gridSize);
            var gridY = -Mathf.RoundToInt(targetPos.y / gridSize);
            var targetId = new Vector2Int(gridX, gridY);
            
            // Check if target position is valid
            var existingNode = currentGraph?.graphData?.nodes?.FirstOrDefault(n => n.id == targetId && n != draggedNode);
            var isValid = existingNode == null;
            
            // Draw target position
            var targetWorldPos = new Vector2(targetId.x * gridSize, -targetId.y * gridSize);
            var nodeSize = Mathf.Max(currentGraph.nodeSize, 20f / zoom);
            
            // Draw preview circle
            var previewColor = isValid ? Color.green : Color.red;
            DrawCircle(targetWorldPos, nodeSize / 2, new Color(previewColor.r, previewColor.g, previewColor.b, 0.3f));
            DrawCircleBorder(targetWorldPos, nodeSize / 2, previewColor, 2f);
            
            // Draw target position label
            var labelRect = new Rect(targetWorldPos.x - 30, targetWorldPos.y + nodeSize / 2 + 5, 60, 20);
            var style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(Mathf.RoundToInt(10), 8),
                normal = { textColor = previewColor },
                fontStyle = FontStyle.Bold
            };
            EditorGUI.LabelField(labelRect, targetId.ToString(), style);
        }

        private void DrawLine(Vector2 from, Vector2 to, Color color, float width)
        {
            var direction = (to - from).normalized;
            var perpendicular = new Vector2(-direction.y, direction.x);
            var halfWidth = width / 2f;

            var vertices = new Vector3[]
            {
                from + perpendicular * halfWidth,
                from - perpendicular * halfWidth,
                to - perpendicular * halfWidth,
                to + perpendicular * halfWidth
            };

            var colors = new Color[] { color, color, color, color };
            var indices = new int[] { 0, 1, 2, 0, 2, 3 };

            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);
            Handles.EndGUI();
        }

        private void DrawArrow(Vector2 from, Vector2 to, Color color, float width)
        {
            var direction = (to - from).normalized;
            var arrowSize = width * 3f;
            var arrowPos = Vector2.Lerp(from, to, 0.8f);

            var perpendicular = new Vector2(-direction.y, direction.x);
            var arrowTip = arrowPos + direction * arrowSize;
            var arrowLeft = arrowPos - direction * arrowSize * 0.5f + perpendicular * arrowSize * 0.5f;
            var arrowRight = arrowPos - direction * arrowSize * 0.5f - perpendicular * arrowSize * 0.5f;

            var vertices = new Vector3[] { arrowTip, arrowLeft, arrowRight };
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);
            Handles.EndGUI();
        }

        private void DrawRect(Rect rect, Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            GUI.DrawTexture(rect, texture);
        }

        private void DrawRectBorder(Rect rect, Color color, float borderWidth)
        {
            // Draw border by drawing 4 rectangles around the edges
            var leftRect = new Rect(rect.x - borderWidth, rect.y - borderWidth, borderWidth, rect.height + borderWidth * 2);
            var rightRect = new Rect(rect.x + rect.width, rect.y - borderWidth, borderWidth, rect.height + borderWidth * 2);
            var topRect = new Rect(rect.x, rect.y - borderWidth, rect.width, borderWidth);
            var bottomRect = new Rect(rect.x, rect.y + rect.height, rect.width, borderWidth);

            DrawRect(leftRect, color);
            DrawRect(rightRect, color);
            DrawRect(topRect, color);
            DrawRect(bottomRect, color);
        }

        private void DrawCircle(Vector2 center, float radius, Color color)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
            Handles.EndGUI();
        }

        private void DrawCircleBorder(Vector2 center, float radius, Color color, float borderWidth)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawWireDisc(center, Vector3.forward, radius + borderWidth / 2);
            Handles.EndGUI();
        }
        
        private void DrawEnemyFacingDirection(Vector2 nodePos, Vector2Int facingDirection, float nodeSize)
        {
            if (facingDirection == Vector2Int.zero) return;
            
            // Calculate direction vector (note: Y is inverted in screen space)
            var direction = new Vector2(facingDirection.x, -facingDirection.y);
            var directionLength = nodeSize * 0.4f;
            var arrowLength = nodeSize * 0.15f;
            
            // Draw direction line
            var endPos = nodePos + direction * directionLength;
            Handles.BeginGUI();
            Handles.color = Color.yellow;
            Handles.DrawLine(nodePos, endPos);
            
            // Draw arrow head
            var perpendicular = new Vector2(-direction.y, direction.x).normalized;
            var arrowLeft = endPos - direction * arrowLength + perpendicular * arrowLength * 0.5f;
            var arrowRight = endPos - direction * arrowLength - perpendicular * arrowLength * 0.5f;
            
            var arrowPoints = new Vector3[] { endPos, arrowLeft, arrowRight };
            Handles.DrawAAConvexPolygon(arrowPoints);
            Handles.EndGUI();
        }

        private Vector2 GetNodeScreenPosition(Vector2Int nodeId)
        {
            var nodeData = currentGraph?.graphData?.nodes?.FirstOrDefault(n => n.id == nodeId);
            if (nodeData != null)
            {
                return nodeData.GetWorldPosition(gridSize);
            }
            return new Vector2(nodeId.x * gridSize, -nodeId.y * gridSize);
        }

        private Vector2 GetNodeScreenPosition(NodeData nodeData)
        {
            return nodeData.GetWorldPosition(gridSize);
        }

        private Vector2Int GetNodeIdFromScreenPosition(Vector2 screenPos)
        {
            // Convert transformed screen position to grid coordinates
            return new Vector2Int(
                Mathf.RoundToInt(screenPos.x / gridSize),
                -Mathf.RoundToInt(screenPos.y / gridSize)
            );
        }

        private Vector2 TransformMousePosition(Vector2 mousePos)
        {
            // Transform mouse position to account for GUI.matrix transformation
            // This reverses the transformation applied in DrawGraph
            var matrix = Matrix4x4.TRS(panOffset, Quaternion.identity, Vector3.one * zoom);
            var inverseMatrix = matrix.inverse;
            var transformedPos = inverseMatrix.MultiplyPoint3x4(new Vector3(mousePos.x, mousePos.y, 0));
            return new Vector2(transformedPos.x, transformedPos.y);
        }

        private void CreateNodeAtPosition(Vector2 screenPos)
        {
            if (currentGraph?.graphData == null) return;

            // Convert transformed screen position to grid coordinates
            var gridX = Mathf.RoundToInt(screenPos.x / gridSize);
            var gridY = -Mathf.RoundToInt(screenPos.y / gridSize); // Negative because Y is inverted
            var nodeId = new Vector2Int(gridX, gridY);
            
            // Check if node already exists
            if (currentGraph.graphData.nodes.Any(n => n.id == nodeId))
                return;

            var newNode = new NodeData(nodeId, NodeType.Normal);
            var command = new AddNodeCommand(currentGraph.graphData, newNode);
            ExecuteCommand(command);
            selectedNode = newNode;
            selectedEdge = null;
        }

        private void StartEdgeCreation(Vector2 screenPos)
        {
            edgeStartNode = FindNodeAtPosition(screenPos);
            
            if (edgeStartNode != null)
            {
                isCreatingEdge = true;
                selectedNode = null;
                selectedEdge = null;
            }
        }

        private void CompleteEdgeCreation(Vector2 screenPos)
        {
            if (!isCreatingEdge || edgeStartNode == null) return;

            var targetNode = FindNodeAtPosition(screenPos);
            
            if (targetNode != null && targetNode != edgeStartNode)
            {
                CreateEdge(edgeStartNode, targetNode);
            }
            
            isCreatingEdge = false;
            edgeStartNode = null;
            CheckSelectionChange();
        }

        private void HandleEdgeModeClick(Vector2 screenPos)
        {
            var clickedNode = FindNodeAtPosition(screenPos);
            
            if (clickedNode == null) 
            {
                modeFeedback = "No node found at click position";
                return;
            }
            
            if (edgeStartNode == null)
            {
                // Start edge creation
                edgeStartNode = clickedNode;
                selectedNode = clickedNode;
                selectedEdge = null;
                modeFeedback = $"Edge creation started from {clickedNode.id}. Click another node to complete.";
                CheckSelectionChange();
            }
            else if (edgeStartNode != clickedNode)
            {
                // Complete edge creation
                CreateEdge(edgeStartNode, clickedNode);
                modeFeedback = $"Edge created from {edgeStartNode.id} to {clickedNode.id}";
                edgeStartNode = null;
                selectedNode = clickedNode;
                CheckSelectionChange();
            }
            else
            {
                // Clicked the same node - cancel edge creation
                edgeStartNode = null;
                selectedNode = clickedNode;
                modeFeedback = "Edge creation cancelled";
                CheckSelectionChange();
            }
        }

        private void SelectAtPosition(Vector2 screenPos)
        {
            // Don't change selection if we're currently panning
            if (isDragging && !isDraggingNode)
                return;

            // First try to select node (nodes are drawn on top)
            var foundNode = FindNodeAtPosition(screenPos);
            if (foundNode != null)
            {
                selectedNode = foundNode;
                selectedEdge = null;
                CheckSelectionChange();
                return;
            }

            // Then try to select edge if no node was found
            var foundEdge = FindEdgeAtPosition(screenPos);
            if (foundEdge != null)
            {
                selectedNode = null;
                selectedEdge = foundEdge;
                CheckSelectionChange();
            }
            else
            {
                // Clear selection if nothing was found
                selectedNode = null;
                selectedEdge = null;
                CheckSelectionChange();
            }
        }

        private void CheckSelectionChange()
        {
            // Check if selection has changed
            if (selectedNode != lastSelectedNode || selectedEdge != lastSelectedEdge)
            {
                // Clear GUI focus to reset text fields
                GUI.FocusControl(null);
                
                // Update last selection
                lastSelectedNode = selectedNode;
                lastSelectedEdge = selectedEdge;
            }
        }

        private NodeData FindNodeAtPosition(Vector2 screenPos)
        {
            if (currentGraph?.graphData?.nodes == null) return null;

            foreach (var nodeData in currentGraph.graphData.nodes)
            {
                var nodePos = GetNodeScreenPosition(nodeData);
                var nodeSize = Mathf.Max(currentGraph.nodeSize, 20f / zoom); // Adjust for zoom
                var selectionRadius = nodeSize / 2 + 5f / zoom; // Add extra selection area, adjust for zoom
                var distance = Vector2.Distance(screenPos, nodePos);
                
                if (distance <= selectionRadius)
                {
                    return nodeData;
                }
            }

            return null;
        }

        private void StartNodeDrag(Vector2 screenPos)
        {
            if (selectedNode == null) return;

            isDraggingNode = true;
            draggedNode = selectedNode;
            
            // Calculate offset from node center to mouse position (both in transformed space)
            var nodePos = GetNodeScreenPosition(selectedNode);
            dragOffset = screenPos - nodePos;
        }

        private void UpdateNodeDrag(Vector2 screenPos)
        {
            if (draggedNode == null) return;

            // Apply drag offset to get the actual target position
            var targetPos = screenPos - dragOffset;
            
            // Convert to grid coordinates
            var gridX = Mathf.RoundToInt(targetPos.x / gridSize);
            var gridY = -Mathf.RoundToInt(targetPos.y / gridSize); // Negative because Y is inverted
            
            // Check if the new position is valid (not occupied by another node)
            var newId = new Vector2Int(gridX, gridY);
            var existingNode = currentGraph?.graphData?.nodes?.FirstOrDefault(n => n.id == newId && n != draggedNode);
            
            if (existingNode == null)
            {
                // Store old ID for edge updates
                var oldId = draggedNode.id;
                
                // Update node position
                draggedNode.id = newId;
                
                // Update all edges that reference this node
                if (currentGraph?.graphData?.edges != null)
                {
                    var updatedEdges = 0;
                    foreach (var edge in currentGraph.graphData.edges)
                    {
                        if (edge.fromId == oldId)
                        {
                            edge.fromId = newId;
                            updatedEdges++;
                        }
                        if (edge.toId == oldId)
                        {
                            edge.toId = newId;
                            updatedEdges++;
                        }
                    }
                    if (updatedEdges > 0)
                    {
                        Debug.Log($"Updated {updatedEdges} edges for node {oldId} -> {newId}");
                    }
                }
                
                EditorUtility.SetDirty(currentGraph);
            }
        }

        private void EndNodeDrag()
        {
            if (isDraggingNode && draggedNode != null)
            {
                // Create move command for the final position
                var command = new MoveNodeCommand(draggedNode, draggedNode.id);
                ExecuteCommand(command);
            }
            
            isDraggingNode = false;
            draggedNode = null;
        }

        private EdgeData FindEdgeAtPosition(Vector2 screenPos)
        {
            if (currentGraph?.graphData?.edges == null) return null;

            var threshold = Mathf.Max(currentGraph.edgeWidth * 2f / zoom, 8f / zoom); // Adjust threshold for zoom
            
            // Cache node lookup for better performance - O(1) instead of O(n) for each edge
            var nodeDict = currentGraph.graphData.nodes.ToDictionary(n => n.id);

            foreach (var edge in currentGraph.graphData.edges)
            {
                if (!nodeDict.TryGetValue(edge.fromId, out var fromNode) || 
                    !nodeDict.TryGetValue(edge.toId, out var toNode))
                    continue;
                
                var fromPos = GetNodeScreenPosition(fromNode);
                var toPos = GetNodeScreenPosition(toNode);

                if (IsPointNearLine(screenPos, fromPos, toPos, threshold))
                {
                    return edge;
                }
            }

            return null;
        }

        private bool IsPointNearLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float threshold)
        {
            var lineVec = lineEnd - lineStart;
            var pointVec = point - lineStart;
            var lineLength = lineVec.magnitude;

            if (lineLength == 0) return false;

            var t = Vector2.Dot(pointVec, lineVec) / (lineLength * lineLength);
            t = Mathf.Clamp01(t);

            var projection = lineStart + t * lineVec;
            return Vector2.Distance(point, projection) <= threshold;
        }

        private void ShowContextMenu(Vector2 screenPos)
        {
            var menu = new GenericMenu();
            var existingNode = FindNodeAtPosition(screenPos);

            if (existingNode != null)
            {
                // Node context menu
                if (existingNode.type != NodeType.Start && existingNode.type != NodeType.Goal)
                {
                    menu.AddItem(new GUIContent("Delete Node"), false, () => DeleteNode(existingNode));
                    menu.AddSeparator("");
                }
                else
                {
                    // Disable deletion for Start and Goal nodes
                    menu.AddDisabledItem(new GUIContent("Delete Node"));
                    menu.AddSeparator("");
                }
                
                foreach (NodeType nodeType in System.Enum.GetValues(typeof(NodeType)))
                {
                    var isSelected = existingNode.type == nodeType;
                    menu.AddItem(new GUIContent($"Set Type/{nodeType}"), isSelected, () => SetNodeType(existingNode, nodeType));
                }
            }
            else
            {
                // Empty space context menu
                menu.AddItem(new GUIContent("Create Node"), false, () => CreateNodeAtPosition(screenPos));
            }

            menu.ShowAsContext();
        }

        private void DeleteNode(NodeData node)
        {
            if (node == null) return;

            // Prevent deletion of Start and Goal nodes
            if (node.type == NodeType.Start || node.type == NodeType.Goal)
            {
                Debug.LogWarning("Start and Goal nodes cannot be deleted.");
                return;
            }

            if (currentGraph?.graphData == null) return;

            var command = new DeleteNodeCommand(currentGraph.graphData, node);
            ExecuteCommand(command);
            
            if (selectedNode == node) selectedNode = null;
            if (edgeStartNode == node) edgeStartNode = null;
            
            CheckSelectionChange();
        }

        private void SetNodeType(NodeData node, NodeType type)
        {
            if (node != null)
            {
                node.type = type;
                EditorUtility.SetDirty(currentGraph);
            }
        }

        private void CreateNewGraph()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create New Graph", "NewGraph", "asset", "Create new graph");
            if (!string.IsNullOrEmpty(path))
            {
                currentGraph = CreateInstance<GraphScriptableObject>();

                // Add default Start and Goal nodes (no edges)
                var startNode = new NodeData(new Vector2Int(0, 0), NodeType.Start);
                var goalNode  = new NodeData(new Vector2Int(2, 0),  NodeType.Goal);
                currentGraph.graphData.nodes.Add(startNode);
                currentGraph.graphData.nodes.Add(goalNode);

                AssetDatabase.CreateAsset(currentGraph, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void SaveGraph()
        {
            if (currentGraph != null)
            {
                EditorUtility.SetDirty(currentGraph);
                AssetDatabase.SaveAssets();
                Debug.Log($"Graph '{currentGraph.graphName}' saved successfully.");
            }
        }

        private void DrawInspector()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            
            if (currentGraph != null)
            {
                EditorGUILayout.LabelField("Graph Properties", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                // Graph info
                currentGraph.graphName = EditorGUILayout.TextField("Graph Name", currentGraph.graphName);
                currentGraph.description = EditorGUILayout.TextField("Description", currentGraph.description);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Nodes: {currentGraph.graphData?.nodes?.Count ?? 0}");
                EditorGUILayout.LabelField($"Edges: {currentGraph.graphData?.edges?.Count ?? 0}");
                
                EditorGUILayout.Space();
                
                // Selected node properties
                if (selectedNode != null)
                {
                    EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);
                    
                    EditorGUI.BeginChangeCheck();
                    var oldId = selectedNode.id;
                    selectedNode.id = EditorGUILayout.Vector2IntField("Grid Position", selectedNode.id);
                    if (EditorGUI.EndChangeCheck())
                    {
                        // Check if the new position is valid
                        var existingNode = currentGraph?.graphData?.nodes?.FirstOrDefault(n => n.id == selectedNode.id && n != selectedNode);
                        if (existingNode != null)
                        {
                            // Revert to previous position if occupied
                            selectedNode.id = oldId;
                        }
                        else
                        {
                            // Update all edges that reference this node
                            if (currentGraph?.graphData?.edges != null)
                            {
                                var updatedEdges = 0;
                                foreach (var edge in currentGraph.graphData.edges)
                                {
                                    if (edge.fromId == oldId)
                                    {
                                        edge.fromId = selectedNode.id;
                                        updatedEdges++;
                                    }
                                    if (edge.toId == oldId)
                                    {
                                        edge.toId = selectedNode.id;
                                        updatedEdges++;
                                    }
                                }
                                if (updatedEdges > 0)
                                {
                                    Debug.Log($"Updated {updatedEdges} edges for node {oldId} -> {selectedNode.id} (inspector)");
                                }
                            }
                            EditorUtility.SetDirty(currentGraph);
                        }
                    }
                    
                    EditorGUILayout.Space();
                    selectedNode.type = (NodeType)EditorGUILayout.EnumPopup("Type", selectedNode.type);
                    selectedNode.isDestroyed = EditorGUILayout.Toggle("Destroyed", selectedNode.isDestroyed);
                    
                    // Enemy-specific properties
                    if (selectedNode.type == NodeType.Enemy)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Enemy Properties", EditorStyles.boldLabel);
                        selectedNode.enemyFacingDirection = EditorGUILayout.Vector2IntField("Facing Direction", selectedNode.enemyFacingDirection);
                        
                        // Quick buttons for common directions
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("↑ Up")) selectedNode.enemyFacingDirection = Vector2Int.up;
                        if (GUILayout.Button("↓ Down")) selectedNode.enemyFacingDirection = Vector2Int.down;
                        if (GUILayout.Button("← Left")) selectedNode.enemyFacingDirection = Vector2Int.left;
                        if (GUILayout.Button("→ Right")) selectedNode.enemyFacingDirection = Vector2Int.right;
                        EditorGUILayout.EndHorizontal();
                        
                        // Enemy state selection
                        EditorGUILayout.Space();
                        selectedNode.enemyState = (GridEnemy.EnemyState)EditorGUILayout.EnumPopup("Behavior State", selectedNode.enemyState);
                        
                        // State description
                        string stateDescription = GetEnemyStateDescription(selectedNode.enemyState);
                        EditorGUILayout.HelpBox(stateDescription, MessageType.Info);
                    }
                    
                    EditorGUILayout.Space();
                    
                    // Show Delete button only if node is NOT Start or Goal
                    if (selectedNode.type != NodeType.Start && selectedNode.type != NodeType.Goal)
                    {
                        if (GUILayout.Button("Delete Node"))
                        {
                            DeleteNode(selectedNode);
                        }
                    }
                }
                
                // Selected edge properties
                if (selectedEdge != null)
                {
                    EditorGUILayout.LabelField("Selected Edge", EditorStyles.boldLabel);
                    EditorGUILayout.Vector2IntField("From", selectedEdge.fromId);
                    EditorGUILayout.Vector2IntField("To", selectedEdge.toId);
                    selectedEdge.type = (EdgeType)EditorGUILayout.EnumPopup("Type", selectedEdge.type);
                    selectedEdge.isUsed = EditorGUILayout.Toggle("Used", selectedEdge.isUsed);
                    
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button("Delete Edge"))
                    {
                        var command = new DeleteEdgeCommand(currentGraph.graphData, selectedEdge);
                        ExecuteCommand(command);
                        selectedEdge = null;
                        CheckSelectionChange();
                    }
                }
                
                // Edge creation
                if ((isCreatingEdge || edgeCreationMode) && edgeStartNode != null)
                {
                    EditorGUILayout.LabelField("Creating Edge", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"From: {edgeStartNode.id}");
                    
                    if (edgeCreationMode)
                    {
                        EditorGUILayout.LabelField("Click on target node to complete");
                        EditorGUILayout.LabelField("Click same node to cancel");
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Click on target node to complete");
                    }
                    
                    if (GUILayout.Button("Cancel"))
                    {
                        isCreatingEdge = false;
                        edgeStartNode = null;
                        CheckSelectionChange();
                    }
                }
                
                // Editor settings
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);
                currentGraph.nodeSize = EditorGUILayout.FloatField("Node Size", currentGraph.nodeSize);
                currentGraph.edgeWidth = EditorGUILayout.FloatField("Edge Width", currentGraph.edgeWidth);
                gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);
            }
            else
            {
                var style = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    normal = { textColor = Color.gray }
                };
                EditorGUILayout.LabelField("No graph selected", style);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawHelpWindow()
        {
            // Calculate position for top-right corner
            var windowWidth = position.width;
            var windowHeight = position.height;
            var helpRect = new Rect(windowWidth - 450, 10, 440, 650);
            GUI.Box(helpRect, "");
            
            var contentRect = new Rect(helpRect.x + 10, helpRect.y + 10, helpRect.width - 20, helpRect.height - 20);
            GUILayout.BeginArea(contentRect);
            
            EditorGUILayout.LabelField("Graph Editor Shortcuts", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Mouse Controls
            EditorGUILayout.LabelField("Mouse Controls:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Left Click: Selection");
            EditorGUILayout.LabelField("• Left Click + Drag on Node: Drag node");
            EditorGUILayout.LabelField("• Left Click + Drag on Empty: Pan view");
            EditorGUILayout.LabelField("• Ctrl + Left Click: Create node");
            EditorGUILayout.LabelField("• Shift + Left Click: Start edge creation");
            EditorGUILayout.LabelField("• Edge Mode: Enable from toolbar, click nodes to create edges");
            EditorGUILayout.LabelField("• Right Click: Context menu");
            EditorGUILayout.LabelField("• Middle Click + Drag: Pan (move view)");
            EditorGUILayout.LabelField("• Mouse Wheel: Zoom in/out");
            EditorGUILayout.LabelField("• Ctrl + Mouse Wheel: Faster zoom");
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Keyboard Controls:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Ctrl + Plus/Minus: Zoom in/out");
            EditorGUILayout.LabelField("• Home: Reset view (zoom and pan)");
            EditorGUILayout.LabelField("• Space: Toggle grid visibility");
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Toolbar Controls:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• +/- buttons: Zoom in/out");
            EditorGUILayout.LabelField("• Reset button: Reset view to default");
            EditorGUILayout.LabelField("• Grid toggle: Show/hide grid");
            EditorGUILayout.LabelField("• Edge Mode toggle: Enable edge creation mode");
            EditorGUILayout.Space();
            
            // Node Types
            EditorGUILayout.LabelField("Node Types:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Normal (White): Standard transition point");
            EditorGUILayout.LabelField("• Start (Green): Starting point");
            EditorGUILayout.LabelField("• Goal (Red): Target point");
            EditorGUILayout.LabelField("• Breakable (Yellow): Breakable node");
            EditorGUILayout.LabelField("• Redirector (Blue): Direction changer");
            EditorGUILayout.LabelField("• Trap (Black): Trap");
            EditorGUILayout.LabelField("• Enemy (Magenta): Stationary Guard with facing direction");
            EditorGUILayout.Space();
            
            // Edge Types
            EditorGUILayout.LabelField("Edge Types:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Standard (White): Bidirectional passage");
            EditorGUILayout.LabelField("• Directed (Cyan): One-way passage");
            EditorGUILayout.LabelField("• Slippery (Orange): Slippery passage");
            EditorGUILayout.LabelField("• Breakable (Red): Breakable passage");
            EditorGUILayout.Space();
            
            // Tips
            EditorGUILayout.LabelField("Tips:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Nodes have minimum 20px display size");
            EditorGUILayout.LabelField("• Edge selection uses wide area");
            EditorGUILayout.LabelField("• Selected elements highlighted in yellow");
            EditorGUILayout.LabelField("• Node dragging snaps to grid");
            EditorGUILayout.LabelField("• Green preview = valid position, Red = occupied");
            EditorGUILayout.LabelField("• Adjust grid size to increase precision");
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Close"))
            {
                showHelpWindow = false;
            }
            
            GUILayout.EndArea();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            // This method is kept for potential future scene view integration
            // Currently, all interaction is handled in the editor window
        }

        private void CreateEdge(NodeData from, NodeData to)
        {
            if (currentGraph?.graphData == null) return;
            
            // Check if edge already exists
            if (currentGraph.graphData.edges.Any(e => 
                (e.fromId == from.id && e.toId == to.id) || 
                (e.fromId == to.id && e.toId == from.id)))
                return;

            var newEdge = new EdgeData(from.id, to.id, EdgeType.Standard);
            var command = new AddEdgeCommand(currentGraph.graphData, newEdge);
            ExecuteCommand(command);
            selectedEdge = newEdge;
            selectedNode = null;
            
            CheckSelectionChange();
            Debug.Log($"Edge created from {from.id} to {to.id}");
            
            // Note: At runtime, EdgeData will be converted to NodeConnection for O(1) performance
        }

        private void DrawModeFeedback()
        {
            // Reset matrix for UI drawing
            GUI.matrix = Matrix4x4.identity;
            
            var style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = Color.yellow },
                fontStyle = FontStyle.Bold
            };
            
            var rect = new Rect(10, position.height - 40, position.width - 320, 30);
            GUI.Box(rect, "");
            EditorGUI.LabelField(rect, modeFeedback, style);
        }

        private void ExecuteCommand(ICommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear(); // Clear redo stack when new command is executed
            EditorUtility.SetDirty(currentGraph);
        }

        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
                EditorUtility.SetDirty(currentGraph);
                modeFeedback = "Undo executed";
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
                EditorUtility.SetDirty(currentGraph);
                modeFeedback = "Redo executed";
            }
        }

        private string GetEnemyStateDescription(GridEnemy.EnemyState state)
        {
            switch (state)
            {
                case GridEnemy.EnemyState.Stationary:
                    return "Enemy stays in place, only detects player in vision range.";
                case GridEnemy.EnemyState.Patrol:
                    return "Enemy patrols back and forth in facing direction.";
                case GridEnemy.EnemyState.MovingTarget:
                    return "Enemy follows player if within 1 edge distance, otherwise becomes stationary.";
                default:
                    return "Unknown enemy state.";
            }
        }
    }
} 