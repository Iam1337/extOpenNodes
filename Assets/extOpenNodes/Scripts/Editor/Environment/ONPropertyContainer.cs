/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;
using UnityEngine.Experimental.UIElements;

using UnityEditor;

using extOpenNodes.Core;

namespace extOpenNodes.Editor.Environment
{
    public class ONPropertyContainer : ONElementContainer<ONProperty>
    {
        #region Private Vars

        private Vector2 _tooltipSize;

        private GUIContent _tooltipContent;

        private float _tooltipXOffset;

        private bool _tooltipInit;

        #endregion

        #region Private Vars

        public ONProperty Property
        {
            get { return element; }
            set { element = value; }
        }

        #endregion

        #region Public Methods

        public ONPropertyContainer(ONProperty property, ONNodeContainer nodeVisual, ONWorkflowEnvironment workflowEditor) : base(property, workflowEditor)
        {
            Parent = nodeVisual;
        }

        public override void Draw()
        {
            var defaultColor = GUI.color;
            var propertyRect = Rect;

            if (propertyRect.Contains(Event.current.mousePosition))
            {
                Environment.FocusedPropertyContainer = this;

                DrawTooltip();
                ProcessEvents();
            }

            if (Environment.FocusedPropertyContainer == this)
            {
                GUI.color = Color.grey;
            }

            GUI.DrawTexture(propertyRect, ONEditorTextures.PropertyTexture);

            var linkSize = new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight) / 2;
            var linkRect = new Rect(propertyRect.center - linkSize / 2f, linkSize);

            GUI.DrawTexture(linkRect, ONEditorTextures.LinkTexture);

            GUI.color = defaultColor;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void ProcessEvents()
        {
            if (Environment.SelectedPropertyContainer == null)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == (int)MouseButton.LeftMouse)
                {
                    Environment.SelectedPropertyContainer = this;
                }
            }

            if (Environment.SelectedPropertyContainer != null && Environment.SelectedPropertyContainer != this)
            {
                if (Event.current.type == EventType.MouseUp && Event.current.button == (int)MouseButton.LeftMouse)
                {
                    var link = ONWorkflowUtils.CreateLink(Environment.Workflow, Environment.SelectedPropertyContainer.Property, Property);
                    if (link != null)
                    {
                        Environment.SaveWorkflow();
                    }

                    Environment.SelectedPropertyContainer = null;

                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == (int)MouseButton.RightMouse)
            {
                ONWorkflowUtils.RemoveLinks(Environment.Workflow, Property);

                Event.current.Use();
            }
        }

        private void DrawTooltip()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (!_tooltipInit)
            {
                var tooltipText = string.Empty;

                //TODO: Make better...
                var valueType = Property.ValueType;
                if (valueType != null)
                {
                    tooltipText = valueType.Name;

                    if (valueType.IsGenericType)
                    {
                        var typeNames = string.Empty;

                        foreach (var types in valueType.GetGenericArguments())
                        {
                            typeNames += ObjectNames.NicifyVariableName(types.Name) + ", ";
                        }

                        if (typeNames.Length > 2)
                            typeNames = typeNames.Remove(typeNames.Length - 2);

                        tooltipText += "<" + typeNames + ">";
                    }
                }
                else
                {
                    var reflectionMember = Property.ReflectionMember;
                    if (reflectionMember != null && reflectionMember.IsMethod())
                    {
                        tooltipText = "Method";
                    }
                }

                _tooltipContent = new GUIContent(tooltipText);

                _tooltipSize = GUI.skin.label.CalcSize(_tooltipContent);
                _tooltipSize.x += EditorGUIUtility.standardVerticalSpacing * 6f;
                _tooltipSize.y += EditorGUIUtility.standardVerticalSpacing * 2f;

                var tooltipSpace = EditorGUIUtility.standardVerticalSpacing;

                _tooltipXOffset = 0;

                if (Property.PropertyType == ONPropertyType.Input)
                {
                    _tooltipXOffset -= tooltipSpace + _tooltipSize.x;
                }
                else if (Property.PropertyType == ONPropertyType.Output)
                {
                    _tooltipXOffset += Size.x + tooltipSpace;
                }

                _tooltipInit = true;
            }

            var tooltipPosition = Position;
            tooltipPosition.x += _tooltipXOffset;

            var tooltipRect = new Rect();
            tooltipRect.size = _tooltipSize;
            tooltipRect.position = tooltipPosition;

            Environment.DrawTooltip(_tooltipContent, tooltipRect);
        }

        #endregion
    }
}