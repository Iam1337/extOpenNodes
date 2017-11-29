/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;
using UnityEngine.Experimental.UIElements;

using UnityEditor;
using UnityEditor.SceneManagement;

using System.Collections.Generic;
using System.Linq;

using extOpenNodes.Core;

using extOpenNodes.Editor.Windows;

namespace extOpenNodes.Editor.Environments
{
    //TODO: Rewrite?
    public class ONWorkflowEnvironment
    {
        #region Extensions

        private class DragTask
        {
            public ONNodeContainer NodeVisual;

            public Vector2 MouseOffset;
        }

        #endregion

        #region Public Vars

        public ONWorkflow Workflow
        {
            get { return _workflow; }
        }

        public Vector2 Position
        {
            get
            {
                if (_workflow != null)
                    return _workflow.EnvironmentPosition;

                return _decoratePosition;
            }
            set
            {
                if (_workflow != null)
                    _workflow.EnvironmentPosition = value;

                _decoratePosition = value;
            }
        }

        public Vector2 Size
        {
            get { return _viewerSize; }
        }

        public ONPropertyContainer FocusedPropertyContainer
        {
            get { return _focusedPropertyContainer; }
            set { _focusedPropertyContainer = value; }
        }

        public ONNodeContainer SelectedNodeContainer
        {
            get
            {
                if (_dragTask != null)
                    return _dragTask.NodeVisual;

                return _selectedNodeContainer;
            }
            set
            {
                _selectedNodeContainer = value;
            }
        }

        public ONPropertyContainer SelectedPropertyContainer
        {
            get { return _selectedPropertyContainer; }
            set { _selectedPropertyContainer = value; }
        }

        public bool IsDrag
        {
            get { return _dragTask != null; }
        }

        #endregion

        #region Private Vars

        private readonly List<ONNodeContainer> _containers = new List<ONNodeContainer>();

        private readonly Color _redColor = new Color(0.8f, 0, 0, 1);

        private ONPropertyContainer _selectedPropertyContainer;

        private ONPropertyContainer _focusedPropertyContainer;

        private ONNodeContainer _selectedNodeContainer;

        private ONWindow _parentWindow;

        private Vector2 _decoratePosition;

        private ONWorkflow _workflow;

        private Vector2 _viewerSize;

        private DragTask _dragTask;

        private string _tooltip;

        #endregion

        #region Public Methods

        public ONWorkflowEnvironment(ONWindow parentWindow, ONWorkflow workflow)
        {
            _parentWindow = parentWindow;
            _workflow = workflow;

            if (_workflow != null)
            {
                _workflow.Register();

                var nodes = _workflow.GetNodes();
                foreach (var node in nodes)
                {
                    _containers.Add(new ONNodeContainer(node, this));
                }
            }
        }

        public void DestroyEditors()
        {
            foreach (var container in _containers)
            {
                container.DestroyEditor();
            }
        }

        public void Update()
        {
            if (_workflow != null)
            {
                _workflow.Process();
            }
        }

        public void Draw(Rect viewerRect)
        {
            _focusedPropertyContainer = null;
            _viewerSize = viewerRect.size;

            GUI.BeginGroup(viewerRect);

            var localPosition = viewerRect;
            localPosition.y = 0;

            GUI.Box(localPosition, GUIContent.none, ONEditorStyles.ViewBackground);

            ONEditorLayout.Grid(localPosition, Position);

            _parentWindow.BeginWindows();

            if (_workflow != null)
            {
                DrawWorkflow(_workflow);
            }

            ProcessEvents(localPosition);

            _parentWindow.EndWindows();

            GUI.Box(localPosition, GUIContent.none, ONEditorStyles.ViewBorder);

            GUI.EndGroup();

            DrawTooltip();
        }

        public void DragNodeContainer(ONNodeContainer nodeVisual)
        {
            if (_dragTask != null)
                return;

            _selectedNodeContainer = null;
            _dragTask = new DragTask();

            var mousePosition = GetEnvironmentMousePosition();
            var nodePosition = nodeVisual.Position;

            _dragTask.NodeVisual = nodeVisual;
            _dragTask.MouseOffset = mousePosition - nodePosition;
        }

        public void DrawTooltip(string tooltip)
        {
            _tooltip = tooltip;
        }

        public void CreateNodeContainer(ONNode node)
        {
            if (_containers.FirstOrDefault(c => c.Node == node) != null)
                return;

            var nodeVisual = new ONNodeContainer(node, this);
            nodeVisual.Focus();

            _selectedNodeContainer = nodeVisual;
            _containers.Add(nodeVisual);
        }

        public void RemoveNodeContainer(ONNodeContainer nodeConteiner)
        {
            if (!_containers.Contains(nodeConteiner))
                return;

            if (_selectedNodeContainer == nodeConteiner)
                _selectedNodeContainer = null;

            nodeConteiner.DestroyEditor();
            ONEditorUtils.ClearGlobalCache();

            var node = nodeConteiner.Node;

            _containers.Remove(nodeConteiner);

            ONWorkflowUtils.RemoveNode(_workflow, node);
        }

        public void ShowSpawnMenu(bool underMouse)
        {
            var menuRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0);
            var nodesContent = new List<GUIContent>();
            var spawnPosition = Vector2.zero;

            if (!underMouse)
            {
                spawnPosition = -Position + Size / 2f;
            }
            else
            {
                spawnPosition = GetEnvironmentMousePosition();
            }

            foreach (var nodeData in ONNodesUtils.NodesDatas)
            {
                nodesContent.Add(nodeData.Content);
            }

            var popupItems = nodesContent.ToArray();

            EditorUtility.DisplayCustomMenu(menuRect, popupItems, -1, NodeSpawnMenuCallback, spawnPosition);
        }

        public void ShowNodeMenu(ONNode node)
        {
            var menuRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0);
            var nodesContent = new List<GUIContent>();

            var component = node.Target;
            var componentType = component.GetType();

            var schemes = ONNodesUtils.GetNodeSchemes(componentType);
            var selectIndex = -1;

            nodesContent.Add(new GUIContent("Create Schema"));

            foreach (var schema in schemes)
            {
                nodesContent.Add(new GUIContent("Change Schema/" + schema.Name));
            }

            var popupItems = nodesContent.ToArray();

            EditorUtility.DisplayCustomMenu(menuRect, popupItems, selectIndex, NodeMenuCallback, node);
        }

        public void SaveWorkflow()
        {
            if (_workflow == null) return;
            if (Application.isPlaying) return;

            EditorUtility.SetDirty(_workflow);
            EditorSceneManager.MarkSceneDirty(_workflow.gameObject.scene);
            AssetDatabase.SaveAssets();
        }

        public void RebuildContainer(ONNode node)
        {
            if (_workflow == null)
                return;

            var container = _containers.FirstOrDefault(c => c.Node == node);
            if (container != null)
            {
                _containers.Remove(container);
                _containers.Add(new ONNodeContainer(node, this));
            }
        }

        #endregion

        #region Private Methods

        // DRAW METHODS
        private void DrawWorkflow(ONWorkflow workflow)
        {
            var nodeVisuals = new ONNodeContainer[_containers.Count];
            _containers.CopyTo(nodeVisuals, 0);

            for (var index = 0; index < nodeVisuals.Length; index++)
            {
                if (nodeVisuals[index].Node == null || 
                    nodeVisuals[index].Node.Target == null)
                {
                    RemoveNodeContainer(nodeVisuals[index]);
                    continue;
                }

                nodeVisuals[index].NodeIndex = index;
                nodeVisuals[index].Draw();
            }

            var links = workflow.GetLinks();
            foreach (var link in links)
            {
                DrawLink(link);
            }

            if (Event.current.type == EventType.Repaint)
            {
                if (_selectedPropertyContainer != null)
                {
                    var color = _redColor;
                    var propertyPosition = _selectedPropertyContainer.Rect.center;

                    if (_selectedPropertyContainer.Property.PropertyType == ONPropertyType.Input)
                    {
                        DrawLink(Event.current.mousePosition, propertyPosition, color);
                    }
                    else if (_selectedPropertyContainer.Property.PropertyType == ONPropertyType.Output)
                    {
                        DrawLink(propertyPosition, Event.current.mousePosition, color);
                    }
                }
            }
        }

        private void DrawLink(ONLink link)
        {
            var color = Color.white;

            if (_focusedPropertyContainer != null)
            {
                if (link.SourceProperty == _focusedPropertyContainer.Property ||
                    link.TargetProperty == _focusedPropertyContainer.Property)
                {
                    color = _redColor;
                }
            }

            //Oh, shi~
            var sourcePosition = link.SourceProperty.Node.ViewerPosition + link.SourceProperty.ViewerPosition + Position + link.SourceProperty.ViewerSize / 2f;
            var targetPosition = link.TargetProperty.Node.ViewerPosition + link.TargetProperty.ViewerPosition + Position + link.TargetProperty.ViewerSize / 2f;

            DrawLink(sourcePosition, targetPosition, color);
        }

        private void DrawTooltip()
        {
            if (string.IsNullOrEmpty(_tooltip))
                return;

            var tooltipContent = new GUIContent(_tooltip);

            var position = Event.current.mousePosition;
            position.x += 15;

            var size = ONEditorStyles.CenterLabel.CalcSize(tooltipContent);
            size.y += 2;
            size.x += EditorGUIUtility.standardVerticalSpacing * 4f;

            var rect = new Rect(position, size);

            GUI.Box(rect, GUIContent.none);
            GUI.Box(rect, tooltipContent, ONEditorStyles.CenterLabel);

            _tooltip = string.Empty;
        }

        // PROCESS EVENTS
        private void ProcessEvents(Rect viewerRect)
        {
            if (viewerRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
                if (Event.current.type == EventType.DragPerform)
                {
                    var spawnPosition = GetEnvironmentMousePosition();

                    foreach (var draggedObject in DragAndDrop.objectReferences)
                    {
                        var component = draggedObject as Component;
                        if (component != null)
                        {
                            if (_workflow == null)
                            {
                                ONWorkflowUtils.CreateWorkflow(true);
                            }

                            var node = ONWorkflowUtils.CreateNode(_workflow, component);
                            node.ViewerPosition = spawnPosition;

                            CreateNodeContainer(node);

                            spawnPosition += new Vector2(EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight * 2);
                        }
                        var monoScript = draggedObject as MonoScript;
                        if (monoScript != null)
                        {
                            var monoType = monoScript.GetClass();
                            if (monoType == null) continue;
                            if (monoType.IsEqualsOrSubclass(typeof(Component)))
                            {
                                if (_workflow == null)
                                {
                                    ONWorkflowUtils.CreateWorkflow(true);
                                }

                                var node = ONWorkflowUtils.CreateNode(_workflow, monoType);
                                node.ViewerPosition = spawnPosition;

                                CreateNodeContainer(node);

                                spawnPosition += new Vector2(EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight * 2);
                            }
                        }
                    }

                    SaveWorkflow();
                    DragAndDrop.PrepareStartDrag();
                }

                // VIEW MOVE
                if (Event.current.type == EventType.MouseDrag &&
                    Event.current.button == (int)MouseButton.RightMouse)
                {
                    Position += Event.current.delta;

                    Event.current.Use();
                }
            }

            if (_workflow == null)
                return;

            if (_dragTask != null)
            {
                // NODE MOVE
                if (Event.current.type == EventType.MouseDrag && 
                    Event.current.button == (int)MouseButton.LeftMouse)
                {
                    var mousePosition = GetEnvironmentMousePosition();

                    _dragTask.NodeVisual.Position = mousePosition - _dragTask.MouseOffset;

                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseUp &&
                    Event.current.button == (int)MouseButton.LeftMouse)
                {
                    _selectedNodeContainer = _dragTask.NodeVisual;
                    _dragTask = null;

                    _selectedNodeContainer.Focus();

                    SaveWorkflow();
                    Event.current.Use();
                }
            }

            if (_selectedNodeContainer != null)
            {
                if (Event.current.type == EventType.KeyDown &&
                    Event.current.keyCode == KeyCode.Backspace)
                {
                    RemoveNodeContainer(_selectedNodeContainer);
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    _selectedNodeContainer = null;
                    GUI.FocusWindow(-1);
                }
            }

            if (_selectedPropertyContainer != null)
            {
                if (Event.current.type == EventType.MouseUp &&
                    Event.current.button == (int)MouseButton.LeftMouse)
                {
                    _selectedPropertyContainer = null;
                }
            }

            if (Event.current.type == EventType.MouseDown &&
                Event.current.clickCount == 2 &&
                Event.current.button == (int)MouseButton.RightMouse)
            {
                    ShowSpawnMenu(true);
                Event.current.Use();
            }
        }

        //HELP METHODS
        private void DrawLink(Vector2 firstPosition, Vector2 secondPosition, Color linkColor)
        {
            var distance = Vector2.Distance(firstPosition, secondPosition);
            if (distance > 0.1f)
            {
                var centerPosition = Mathf.Min(Mathf.Abs(firstPosition.y - secondPosition.y), 100);

                var firstTangent = firstPosition + new Vector2(centerPosition, 0);
                var secondTangent = secondPosition - new Vector2(centerPosition, 0);

                Handles.DrawBezier(firstPosition, secondPosition, firstTangent, secondTangent, linkColor, null, 2);
            }

            var defaultColor = GUI.color;

            GUI.color = linkColor;

            var linkSize = new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight) / 2;
            var linkRect = new Rect(firstPosition - linkSize / 2f, linkSize);

            GUI.DrawTexture(linkRect, ONEditorTextures.LinkTexture);

            linkRect = new Rect(secondPosition - linkSize / 2f, linkSize);

            GUI.DrawTexture(linkRect, ONEditorTextures.LinkTexture);

            GUI.color = defaultColor;
        }

        private Vector2 GetEnvironmentMousePosition()
        {
            return Event.current.mousePosition - Position;
        }

        private void NodeSpawnMenuCallback(object userData, string[] options, int select)
        {
            var nodeData = ONNodesUtils.NodesDatas[select];

            var componentType = nodeData.ComponentType;
            if (componentType == null) return;

            if (_workflow == null)
            {
                ONWorkflowUtils.CreateWorkflow(true);
            }

            var node = ONWorkflowUtils.CreateNode(_workflow, componentType);
            node.ViewerPosition = (Vector2)userData;

            CreateNodeContainer(node);
            SaveWorkflow();
        }

        private void NodeMenuCallback(object userData, string[] options, int select)
        {
            var node = (ONNode)userData;
            var component = node.Target;
            var componentType = component.GetType();

            if (select == 0)
            {
                ONNodesUtils.CreateSchema(componentType);
            }
            else
            {
                var index = select - 1;
                var schemes = ONNodesUtils.GetNodeSchemes(componentType);

                ONNodesUtils.RebuildNode(_workflow, node, schemes[index]);

                RebuildContainer(node);
            }
        }

        #endregion
    }
}
