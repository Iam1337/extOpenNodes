/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;
using UnityEngine.Experimental.UIElements;

using UnityEditor;
using UnityEditor.SceneManagement;

using System.Linq;
using System.Collections.Generic;

using extOpenNodes.Core;

using extOpenNodes.Editor.Windows;

namespace extOpenNodes.Editor.Environment
{
    public class ONWorkflowEnvironment
    {
        #region Extensions

        private class DragTask
        {
            public ONNodeContainer NodeVisual;

            public Vector2 MouseOffset;
        }

        private class Tooltip
        {
            public GUIContent Content;

            public Rect Rect;
        }

        #endregion

        #region Public Vars

        public ONWorkflow Workflow
        {
            get { return _workflow; }
            set
            {
                _workflow = value;

                if (_workflow != null)
                    _workflow.Register();

                _selectedNodeContainer = null;
                _dragTask = null;

                InitWorkflow();
            }
        }

        public Vector2 PositionOffset
        {
            get { return _positionOffset; }
            set { _positionOffset = value; }
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

        private readonly Dictionary<ONNode, ONNodeContainer> _containersDictionary = new Dictionary<ONNode, ONNodeContainer>();

        private readonly Color _redColor = new Color(0.8f, 0, 0, 1);

        private ONPropertyContainer _selectedPropertyContainer;

        private ONPropertyContainer _focusedPropertyContainer;

        private ONNodeContainer _selectedNodeContainer;

        private readonly ONWindow _parentWindow;

        private Vector2 _positionOffset;

        private ONWorkflow _workflow;

        private Vector2 _clickOffset;

        private Vector2 _viewerSize;

        private DragTask _dragTask;

        private Tooltip _tooltip;

        #endregion

        #region Public Methods

        public ONWorkflowEnvironment(ONWindow parentWindow)
        {
            _parentWindow = parentWindow;

            ONNodesDictionary.LoadLibrary();
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

            ONEditorLayout.DrawGrid(localPosition, _positionOffset);

            _parentWindow.BeginWindows();

            if (_workflow != null)
            {
                DrawWorkflow(_workflow);
            }

            ProcessEvents(localPosition);

            _parentWindow.EndWindows();

            GUI.Box(localPosition, GUIContent.none, ONEditorStyles.ViewBorder);

            GUI.EndGroup();

            if (_tooltip != null)
            {
                var rect = _tooltip.Rect;
                rect.position += viewerRect.position;

                GUI.Box(rect, _tooltip.Content);

                _tooltip = null;
            }
        }

        public void DestroyInspectors()
        {
            foreach (var nodeVisual in _containersDictionary.Values)
            {
                nodeVisual.DestroyEditor();
            }
        }

        public void DragNodeContainer(ONNodeContainer nodeVisual)
        {
            if (_dragTask != null)
                return;

            _selectedNodeContainer = null;
            _dragTask = new DragTask();

            var mousePosition = GetViewMousePosition();
            var nodePosition = nodeVisual.Position;

            _dragTask.NodeVisual = nodeVisual;
            _dragTask.MouseOffset = mousePosition - nodePosition;
        }

        public void DrawTooltip(GUIContent content, Rect tooltipRect)
        {
            _tooltip = new Tooltip();
            _tooltip.Content = content;
            _tooltip.Rect = tooltipRect;
        }

        public void CreateNodeContainer(ONNode node)
        {
            if (_containersDictionary.ContainsKey(node))
                return;

            var nodeVisual = new ONNodeContainer(node, this);
            nodeVisual.Focus();

            _selectedNodeContainer = nodeVisual;
            _containersDictionary.Add(node, nodeVisual);
        }

        public void RemoveNodeContainer(ONNodeContainer nodeVisual)
        {
            if (_workflow == null && !_containersDictionary.ContainsValue(nodeVisual))
                return;

            if (_selectedNodeContainer == nodeVisual)
                _selectedNodeContainer = null;

            nodeVisual.DestroyEditor();
            ONEditorUtils.ClearGlobalCache();

            var node = nodeVisual.Element;

            _containersDictionary.Remove(node);

            ONWorkflowUtils.RemoveNode(_workflow, node);
        }

        public void ShowSpawnMenu(Rect menuRect)
        {
            var popupItems = ONNodesDictionary.Dictionary.Keys.ToArray();
            var spawnPosition = -PositionOffset + Size / 2f;

            EditorUtility.DisplayCustomMenu(menuRect, popupItems, -1, MenuCreateNode, spawnPosition);
        }

        public void SaveWorkflow()
        {
            if (_workflow == null) return;
            if (Application.isPlaying) return;

            EditorUtility.SetDirty(_workflow);
            EditorSceneManager.MarkSceneDirty(_workflow.gameObject.scene);
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        // DRAW METHODS
        private void DrawWorkflow(ONWorkflow workflow)
        {
            var nodeVisuals = new ONNodeContainer[_containersDictionary.Count];
            _containersDictionary.Values.CopyTo(nodeVisuals, 0);

            for (var index = 0; index < nodeVisuals.Length; index++)
            {
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

                    if (_selectedPropertyContainer.Element.PropertyType == ONPropertyType.Input)
                    {
                        DrawLink(Event.current.mousePosition, propertyPosition, color);
                    }
                    else if (_selectedPropertyContainer.Element.PropertyType == ONPropertyType.Output)
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
                if (link.SourceProperty == _focusedPropertyContainer.Element || link.TargetProperty == _focusedPropertyContainer.Element)
                {
                    color = _redColor;
                }
            }

            //Oh, shi~
            var sourcePosition = link.SourceProperty.Node.ViewerPosition + link.SourceProperty.ViewerPosition + _positionOffset + link.SourceProperty.ViewerSize / 2f;
            var targetPosition = link.TargetProperty.Node.ViewerPosition + link.TargetProperty.ViewerPosition + _positionOffset + link.TargetProperty.ViewerSize / 2f;

            DrawLink(sourcePosition, targetPosition, color);
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
                    var spawnPosition = GetViewMousePosition();

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
                if (Event.current.type == EventType.MouseDrag && Event.current.button == (int)MouseButton.RightMouse)
                {
                    _positionOffset += Event.current.delta;

                    Event.current.Use();
                }
            }

            if (_workflow == null)
                return;

            if (_dragTask != null)
            {
                // NODE MOVE
                if (Event.current.type == EventType.MouseDrag && Event.current.button == (int)MouseButton.LeftMouse)
                {
                    var mousePosition = GetViewMousePosition();

                    _dragTask.NodeVisual.Position = mousePosition - _dragTask.MouseOffset;

                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == (int)MouseButton.LeftMouse)
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
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Backspace)
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
                if (Event.current.type == EventType.MouseUp && Event.current.button == (int)MouseButton.LeftMouse)
                {
                    _selectedPropertyContainer = null;
                }
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == (int)MouseButton.RightMouse)
            {
                _clickOffset = _positionOffset;
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == (int)MouseButton.RightMouse)
            {
                if (Vector2.Distance(_clickOffset, _positionOffset) < 0.1f && Event.current.clickCount == 1)
                {
                    var popupItems = ONNodesDictionary.Dictionary.Keys.ToArray();
                    var spawnPosition = GetViewMousePosition();

                    var menuRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0);

                    EditorUtility.DisplayCustomMenu(menuRect, popupItems, -1, MenuCreateNode, spawnPosition);
                }
            }
        }

        //HELP METHODS
        private void DrawLink(ONPropertyContainer propertyVisual, Vector2 position, Color linkColor)
        {
            var propertyPosition = propertyVisual.Rect.center;

            if (propertyVisual.Element.PropertyType == ONPropertyType.Input)
            {
                DrawLink(position, propertyPosition, linkColor);
            }
            else if (propertyVisual.Element.PropertyType == ONPropertyType.Output)
            {
                DrawLink(propertyPosition, position, linkColor);
            }
        }

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

        private Vector2 GetViewMousePosition()
        {
            var mousePosition = Event.current.mousePosition;
            return mousePosition - _positionOffset;
        }

        private void InitWorkflow()
        {
            DestroyInspectors();

            _containersDictionary.Clear();

            if (_workflow == null)
                return;

            var nodes = _workflow.GetNodes();
            foreach (var node in nodes)
            {
                _containersDictionary.Add(node, new ONNodeContainer(node, this));
            }
        }

        private void MenuCreateNode(object userData, string[] options, int select)
        {
            var type = ONNodesDictionary.GetType(options[select]);
            if (type == null) return;

            if (_workflow == null)
            {
                ONWorkflowUtils.CreateWorkflow(true);
            }

            var node = ONWorkflowUtils.CreateNode(_workflow, type);
            node.ViewerPosition = (Vector2)userData;

            CreateNodeContainer(node);
            SaveWorkflow();
        }

        #endregion
    }
}