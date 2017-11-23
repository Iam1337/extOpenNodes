/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;
using UnityEngine.Experimental.UIElements;

using UnityEditor;

using System.Collections.Generic;

using extOpenNodes.Core;

namespace extOpenNodes.Editor.Environment
{
    public class ONNodeContainer : ONElementContainer<ONNode>
    {
        #region Private Static Vars

        private static readonly GUIContent _inspectorsContent = new GUIContent("Inspector:");

        private static readonly Vector2 _propertySize = new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);

        private static readonly float _propertyStep = EditorGUIUtility.standardVerticalSpacing;

        #endregion

        #region Public Vars

        public int NodeIndex
        {
            get { return _nodeIndex; }
            set { _nodeIndex = value; }
        }

        public ONNode Node
        {
            get { return element; }
            set { element = value; }
        }

        #endregion

        #region Private Vars

        private Rect _drawZone;

        private ONPropertyContainer[] _propertyVisuals;

        private UnityEditor.Editor _targetEditor;

        private Rect _propertiesRect;

        private Vector2 _clickOffset;

        private Vector2 _size;

        private bool _initNode;

        private int _nodeIndex;

        #endregion

        #region Public Methods

        public ONNodeContainer(ONNode node, ONWorkflowEnvironment workflowEditor) : base(node, workflowEditor)
        {
            var propertyVisuals = new List<ONPropertyContainer>();

            var inputProperties = node.InputProperties;
            foreach (var property in inputProperties)
            {
                propertyVisuals.Add(new ONPropertyContainer(property, this, Environment));
            }

            var outputProperties = node.OutputProperties;
            foreach (var property in outputProperties)
            {
                propertyVisuals.Add(new ONPropertyContainer(property, this, Environment));
            }

            _propertyVisuals = propertyVisuals.ToArray();
        }

        public void DestroyEditor()
        {
            if (_targetEditor != null)
            {
                Object.DestroyImmediate(_targetEditor);
                ONEditorUtils.ClearGlobalCache();
            }

            _targetEditor = null;
        }

        public void Focus()
        {
            if (Environment.IsDrag)
                return;

            GUI.FocusWindow(_nodeIndex);
            GUI.BringWindowToFront(_nodeIndex);

            Environment.SelectedNodeContainer = this;
        }

        public override void Draw()
        {
            var viewedRect = new Rect(-Environment.PositionOffset, Environment.Size);
            _drawZone.position = LocalPosition;

            if (_initNode && !viewedRect.Overlaps(_drawZone))
                return;

            var windowName = Node.Name;

            var nodeRect = new Rect(Position, new Vector2(150, 100));
            nodeRect.height = EditorGUIUtility.singleLineHeight;
            nodeRect = GUILayout.Window(_nodeIndex, nodeRect, DrawNodeWindow, windowName);

            //TODO: Shitty hack...
            GUI.BringWindowToBack(_nodeIndex);

            if (Event.current.type == EventType.Repaint)
            {
                Size = nodeRect.size;

                if (_size != Size)
                {
                    _size = Size;
                    _initNode = false;
                }

                if (!_initNode)
                {
                    var inputCount = 0;
                    var outputCount = 0;

                    var propertiesWidth = _propertySize.x + _propertyStep;
                    var propertyOffset = (EditorGUIUtility.singleLineHeight + _propertyStep) * 2 + 1f;

                    _propertiesRect = new Rect(Vector2.zero, nodeRect.size);
                    _propertiesRect.position += new Vector2(-propertiesWidth, propertyOffset);
                    _propertiesRect.size += new Vector2(propertiesWidth * 2, -propertyOffset);

                    var inputXPosition = 0;
                    var outputXPosition = _propertiesRect.width - _propertySize.x;

                    foreach (var propertyVisual in _propertyVisuals)
                    {
                        var counter = 0;
                        var xPosition = 0f;

                        if (propertyVisual.Property.PropertyType == ONPropertyType.Input)
                        {
                            counter = inputCount;
                            xPosition = inputXPosition;
                            inputCount++;
                        }
                        else
                        {
                            counter = outputCount;
                            xPosition = outputXPosition;
                            outputCount++;
                        }

                        var propertyRect = new Rect();
                        propertyRect.position = _propertiesRect.position + new Vector2(xPosition, _propertyStep + (counter * (_propertySize.y + _propertyStep)));
                        propertyRect.size = _propertySize;

                        propertyVisual.LocalRect = propertyRect;
                    }

                    var viewerRect = LocalRect;
                    _propertiesRect.position += LocalPosition;
                    viewerRect.xMax = Mathf.Max(viewerRect.xMax, _propertiesRect.xMax);
                    viewerRect.xMin = Mathf.Min(viewerRect.xMin, _propertiesRect.xMin);
                    viewerRect.yMax = Mathf.Max(viewerRect.yMax, _propertiesRect.yMax);
                    viewerRect.yMin = Mathf.Min(viewerRect.yMin, _propertiesRect.yMin);

                    _drawZone = viewerRect;
                    _initNode = true;
                }
            }

            if (Rect.Contains(Event.current.mousePosition))
            {
                ProcessEvents();
            }

            foreach (var propertyVisual in _propertyVisuals)
            {
                propertyVisual.Draw();
            }
        }

        public ONPropertyContainer GetPropertyVisual(ONProperty property)
        {
            foreach (var propertyVisual in _propertyVisuals)
            {
                if (propertyVisual.Property == property)
                {
                    return propertyVisual;
                }
            }

            return null;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void ProcessEvents()
        {
            if (Environment.IsDrag) return;

            if (Event.current.type == EventType.MouseDown && Event.current.button == (int)MouseButton.LeftMouse)
            {
                Environment.DragNodeContainer(this);
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == (int)MouseButton.RightMouse)
            {
                _clickOffset = Environment.PositionOffset;

                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == (int)MouseButton.RightMouse)
            {
                if (Vector2.Distance(_clickOffset, Environment.PositionOffset) < 0.1f && Event.current.clickCount == 1)
                {
                    //TODO: Node settings.
                }

                Event.current.Use();
            }
        }

        private void DrawNodeWindow(int nodeId)
        {
            var defaultColor = GUI.color;

            if (Node.Target == null)
            {
                Environment.RemoveNodeContainer(this);
                DestroyEditor();
                return;
            }

            if (_targetEditor != null && _targetEditor.target != Node.Target)
            {
                DestroyEditor();
            }

            if (_targetEditor == null)
            {
                _targetEditor = UnityEditor.Editor.CreateEditor(Node.Target);
            }

            if (!Node.CustomInspector)
            {
                ONEditorLayout.DrawIOHeader(Node);

                GUI.color = Node.HideInspector ? Color.grey : defaultColor;
                var hideButton = GUILayout.Button(_inspectorsContent, GUILayout.Height(18));
                if (hideButton)
                {
                    Node.HideInspector = !Node.HideInspector;
                }
                GUI.color = defaultColor;
            }

            if (!Node.HideInspector)
            {
                _targetEditor.OnInspectorGUI();
            }

            GUI.color = defaultColor;
        }

        #endregion
    }
}