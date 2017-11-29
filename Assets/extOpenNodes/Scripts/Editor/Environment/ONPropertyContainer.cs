/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;
using UnityEngine.Experimental.UIElements;

using UnityEditor;

using extOpenNodes.Core;

namespace extOpenNodes.Editor.Environments
{
    public class ONPropertyContainer : ONElementContainer<ONProperty>
    {
        #region Private Vars

        private string _tooltip;

        #endregion

        #region Private Vars

        public ONProperty Property
        {
            get { return element; }
        }

        #endregion

        #region Public Methods

        public ONPropertyContainer(ONProperty property, ONNodeContainer nodeContainer, ONWorkflowEnvironment workflowEditor) : base(property, workflowEditor)
        {
            Parent = nodeContainer;

            if (Property.ValueType != null)
            {
                _tooltip = ONEditorUtils.TypeFriendlyName(Property.ValueType);
            }
            if (Property.ReflectionMember != null &&
                Property.ReflectionMember.IsMethod())
            {
                _tooltip = ONEditorUtils.MemberFrendlyName(Property.ReflectionMember.MemberInfo);
            }
        }

        public override void Draw()
        {
            var defaultColor = GUI.color;
            var propertyRect = Rect;

            if (propertyRect.Contains(Event.current.mousePosition))
            {
                Environment.FocusedPropertyContainer = this;
                Environment.DrawTooltip(_tooltip);

                ProcessEvents();

                GUI.color = Color.grey;
            }

            GUI.DrawTexture(propertyRect, ONEditorTextures.PropertyTexture);

            var linkSize = new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight) / 2;
            var linkRect = new Rect(propertyRect.center - linkSize / 2f, linkSize);

            GUI.DrawTexture(linkRect, ONEditorTextures.LinkTexture);

            GUI.color = defaultColor;
        }

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

        #endregion
    }
}