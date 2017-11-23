/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using extOpenNodes.Editor.Windows;

namespace extOpenNodes.Editor.Editors
{
    [CustomEditor(typeof(ONWorkflow), true)]
    public class ONWorkflowEditor : UnityEditor.Editor
    {
        #region Private Static Vars

        private static readonly GUIContent _openContent = new GUIContent("Open Workflow");

        #endregion

        #region Public Methods

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal("box");

            var open = GUILayout.Button(_openContent, GUILayout.Height(50f));
            if (open)
            {
                ONWindowWorkflow.OpenWorkflow((ONWorkflow)target);
            }

            GUILayout.EndHorizontal();
        }

        #endregion
    }
}