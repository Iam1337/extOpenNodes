/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System.Collections;

using extOpenNodes.Editor.Environments;
using extOpenNodes.Editor.Windows;

namespace extOpenNodes.Editor.Panels
{
    public class ONWorkflowPanel : ONPanel
    {
        #region Static Private Vars

        private static readonly GUIContent _addNodeContent = new GUIContent("Add Node");

        private static readonly GUIContent _resetViewPositionContent = new GUIContent("Reset View Position");

        private static readonly GUIContent _removeNodeContent = new GUIContent("Remove Node");

        private static readonly GUIContent _noneContent = new GUIContent("- None -");

        #endregion

        #region Public Vars

        public ONWorkflowEnvironment Environment
        {
            get { return _environment; }
        }

        #endregion

        #region Private Vars

        private ONWorkflowEnvironment _environment;

        #endregion

        #region Public Methods

        public ONWorkflowPanel(ONWindow parentWindow, string panelId) : base(parentWindow, panelId)
        {
            _environment = new ONWorkflowEnvironment(parentWindow, null);
        }

        public override void Update()
        {
            if (_environment != null)
            {
                _environment.Update();
                _parentWindow.Repaint();
            }
        }

        public void SetWorkflow(ONWorkflow workflow)
        {
            if (_environment != null)
            {
                _environment.DestroyEditors();
                _environment = null;
            }

            _environment = new ONWorkflowEnvironment(ParentWindow, workflow);
        }

        public ONWorkflow GetWorkflow()
        {
            if (_environment != null)
            {
                return _environment.Workflow;
            }

            return null;
        }

        #endregion

        #region Protected Methods

        protected override void DrawContent(Rect contentRect)
        {
            var defaultColor = GUI.color;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var addNode = GUILayout.Button(_addNodeContent, EditorStyles.toolbarDropDown);
            if (addNode)
            {
                if (_environment != null)
                    _environment.ShowSpawnMenu(false);
            }

            GUILayout.Space(5f);

            var resetPos = GUILayout.Button(_resetViewPositionContent, EditorStyles.toolbarButton);
            if (resetPos)
            {
                StopAllCoroutines();
                StartCoroutine(ResetPositionCoroutine());
            }

            GUILayout.FlexibleSpace();

            var workflow = _environment.Workflow;
            if (workflow != null)
            {
                GUI.enabled = _environment.SelectedNodeContainer != null;
                GUI.color = Color.red;

                var removeNode = GUILayout.Button(_removeNodeContent, EditorStyles.toolbarButton);
                if (removeNode)
                {
                    _environment.RemoveNodeContainer(_environment.SelectedNodeContainer);
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

            _environment.Draw(contentRect);
        }

        #endregion

        #region Private Methods

        private IEnumerator ResetPositionCoroutine()
        {
            var velocity = Vector2.zero;
            var distance = Vector2.Distance(_environment.Position, Vector2.zero);
            var startTime = EditorApplication.timeSinceStartup;

            while (distance > 0.01f)
            {
                var timeDelta = (float)(EditorApplication.timeSinceStartup - startTime);

                _environment.Position = Vector2.SmoothDamp(_environment.Position, Vector2.zero, ref velocity, 0.2f, 100, timeDelta);

                distance = Vector2.Distance(_environment.Position, Vector2.zero);

                yield return null;
            }

            _environment.Position = Vector2.zero;
        }

        #endregion
    }
}