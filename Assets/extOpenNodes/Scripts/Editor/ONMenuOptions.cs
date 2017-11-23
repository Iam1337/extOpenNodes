/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEditor;

namespace extOpenNodes.Editor
{
    public static class ONMenuOptions
    {
        #region Static Public Methods

        [MenuItem("GameObject/extOpenNodes/Workflow", false, 40)]
        public static void AddManager(MenuCommand menuCommand)
        {
            ONWorkflowUtils.CreateWorkflow(false);
        }

        #endregion
    }
}