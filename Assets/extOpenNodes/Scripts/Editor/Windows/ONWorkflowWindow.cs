/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;
//using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.Build;
//using UnityEditor.SceneManagement;

using extOpenNodes.Editor.Panels;


namespace extOpenNodes.Editor.Windows
{
    public class ONWorkflowWindow : ONWindow<ONWorkflowWindow, ONWorkflowPanel>
    {
        #region Extensions

        public class PreprocessBuild : IPreprocessBuild
        {
            #region Public Vars

            public int callbackOrder { get { return 0; } }

            #endregion

            #region Public Methods

            public void OnPreprocessBuild(BuildTarget target, string path)
            {
                if (Instance != null)
                {
                    var environment = Instance.rootPanel.Environment;
                    if (environment != null)
                    {
                        environment.DestroyEditors();
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Static Public Methods

        [MenuItem("Window/extOpenNodes/Workflow", false, 0)]
        public static void ShowWindow()
        {
            Instance.titleContent = new GUIContent("Workflow Editor", ONEditorTextures.IronWall);
            Instance.minSize = new Vector2(550, 200);
            Instance.Show();
        }

        public static void OpenWorkflow(ONWorkflow workflow)
        {
            ShowWindow();

            Instance.Focus();
            Instance.rootPanel.SetWorkflow(workflow);

            if (workflow == null)
            {
                ONEditorSettings.SetInt(Instance._currentWorkflow, 0);
            }
            else
            {
                ONEditorSettings.SetInt(Instance._currentWorkflow, workflow.GetInstanceID());
            }
        }

        #endregion

        #region Private Vars

        private readonly string _currentWorkflow = ONEditorSettings.Workflow + "currentWorkflow";

        #endregion

        #region Unity Methods

        protected override void Update()
        {
            if (rootPanel.GetWorkflow() == null)
            {
                LoadWindowSettings();
            }

            base.Update();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            //EditorSceneManager.sceneClosing += OnSceneClosingCallback;
        }

        protected override void OnDisable()
        {
            rootPanel.Environment.DestroyEditors();

            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //EditorSceneManager.sceneClosing -= OnSceneClosingCallback; 
        }

        #endregion

        #region Protected Methods

        protected override void LoadWindowSettings()
        {
            base.LoadWindowSettings();

            var instanceId = ONEditorSettings.GetInt(_currentWorkflow, 0);
            var workflow = EditorUtility.InstanceIDToObject(instanceId) as ONWorkflow;
            if (workflow != null) rootPanel.SetWorkflow(workflow);
        }

        protected override void SaveWindowSettings()
        {
            var workflow = rootPanel.GetWorkflow();
            if (workflow != null) ONEditorSettings.SetInt(_currentWorkflow, workflow.GetInstanceID());

            base.SaveWindowSettings();
        }

        #endregion

        #region Private Methods

        /*
        private void OnSceneClosingCallback(Scene scene, bool removingScene)
        {
            var workflow = rootPanel.GetWorkflow();
            if (workflow != null)
            {
                var workflowScene = workflow.gameObject.scene;
                if (workflowScene == scene)
                {
                    rootPanel.SetWorkflow(null);
                }
            }
        }
        */

        #endregion
    }
}