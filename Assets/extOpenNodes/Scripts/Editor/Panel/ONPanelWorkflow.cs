/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using extOpenNodes.Editor.Environment;
using extOpenNodes.Editor.Windows;

namespace extOpenNodes.Editor.Panels
{
    public class ONPanelWorkflow : ONPanel
    {
        #region Static Private Vars

        private static readonly GUIContent _addNodeContent = new GUIContent("Add Node");

        private static readonly GUIContent _resetViewPositionContent = new GUIContent("Reset View Position");

        private static readonly GUIContent _removeNodeContent = new GUIContent("Remove Node");

        private static readonly GUIContent _noneContent = new GUIContent("- None -");

        #endregion

        #region Public Vars

        public Vector2 PositionOffset
        {
            get { return _workflowEditor.PositionOffset; }
            set { _workflowEditor.PositionOffset = value; }
        }

        public ONWorkflowEnvironment WorkflowEditor
        {
            get { return _workflowEditor; }
        }

        public ONWorkflow Workflow
        {
            get { return _workflowEditor.Workflow; }
            set { _workflowEditor.Workflow = value; }
        }

        #endregion

        #region Private Vars

        private ONWorkflowEnvironment _workflowEditor;

        #endregion

        #region Public Methods

        public ONPanelWorkflow(ONWindow parentWindow, string panelId) : base(parentWindow, panelId)
        {
            _workflowEditor = new ONWorkflowEnvironment(parentWindow);
        }

        #endregion

        #region Protected Methods

        public override void Update()
        {
            _workflowEditor.Update();
            _parentWindow.Repaint();
        }

        protected override void DrawContent(Rect contentRect)
        {
            var defaultColor = GUI.color;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var addNode = GUILayout.Button(_addNodeContent, EditorStyles.toolbarDropDown);
            if (addNode)
            {
                var menuRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0);

                if (_workflowEditor != null)
                    _workflowEditor.ShowSpawnMenu(menuRect);
            }

            GUILayout.Space(5f);

            var resetPos = GUILayout.Button(_resetViewPositionContent, EditorStyles.toolbarButton);
            if (resetPos)
            {
                _workflowEditor.PositionOffset = Vector2.zero;
            }

            GUILayout.FlexibleSpace();

            var workflow = _workflowEditor.Workflow;
            if (workflow != null)
            {
                GUI.enabled = _workflowEditor.SelectedNodeContainer != null;
                GUI.color = Color.red;

                var removeNode = GUILayout.Button(_removeNodeContent, EditorStyles.toolbarButton);
                if (removeNode)
                {
                    _workflowEditor.RemoveNodeContainer(_workflowEditor.SelectedNodeContainer);
                }

                GUI.color = defaultColor;
                GUI.enabled = true;

                GUILayout.Space(5f);

                GUILayout.Label("Name: " + workflow.name);
            }
            else
            {
                GUILayout.Label(_noneContent);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            var toolbarRect = GUILayoutUtility.GetLastRect();
            contentRect.y += toolbarRect.height;
            contentRect.height -= toolbarRect.height;

            GUI.color = defaultColor;

            _workflowEditor.Draw(contentRect);
        }

        #endregion
    }
}