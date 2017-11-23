/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using extOpenNodes.Editor.Panels;

namespace extOpenNodes.Editor.Windows
{
    public class ONWindowWorkflow : ONWindow<ONWindowWorkflow, ONPanelWorkflow>
    {
        #region Static Public Methods

        [MenuItem("Window/extOpenNodes/Workflow Window", false, 0)]
        public static void ShowWindow()
        {
            Instance.titleContent = new GUIContent("ON Workflow", ONEditorTextures.IronWall);
            Instance.minSize = new Vector2(550, 200);
            Instance.Show();
        }

        public static void OpenWorkflow(ONWorkflow workflow)
        {
            ShowWindow();

            Instance.Focus();
            Instance.rootPanel.Workflow = workflow;

            if (workflow == null) ONEditorSettings.SetInt(Instance._lastWorkflowSettings, 0);
            else ONEditorSettings.SetInt(Instance._lastWorkflowSettings, workflow.GetInstanceID());
        }

        #endregion

        #region Private Vars

        private readonly string _lastWorkflowSettings = ONEditorSettings.Workflow + "lastworkflow";

        private readonly string _positionOffsetSettings = ONEditorSettings.Workflow + "offset";

        #endregion

        #region Unity Methods

        protected override void Update()
        {
            if (rootPanel.Workflow == null)
            {
                var instanceId = ONEditorSettings.GetInt(_lastWorkflowSettings, 0);
                var workflow = EditorUtility.InstanceIDToObject(instanceId) as ONWorkflow;
                if (workflow != null) rootPanel.Workflow = workflow;
            }

            base.Update();
        }

        protected override void OnDisable()
        {
            rootPanel.WorkflowEditor.DestroyInspectors();

            base.OnDisable();
        }

        #endregion

        #region Protected Methods

        protected override void LoadWindowSettings()
        {
            rootPanel.PositionOffset = ONEditorSettings.GetVector2(_positionOffsetSettings, Vector2.zero);
        }

        protected override void SaveWindowSettings()
        {
            base.SaveWindowSettings();

            ONEditorSettings.SetVector2(_positionOffsetSettings, rootPanel.PositionOffset);
        }

        #endregion
    }
}