/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEditor;

using System;
using System.Reflection;

namespace extOpenNodes.Editor
{
    public static class ONEditorUtils
    {
        #region Static Private Methods

        private static MethodInfo _clearGCMethodInfo;

        #endregion

        #region Static Public Methods

        public static void ClearGlobalCache()
        {
            if (_clearGCMethodInfo == null)
            {
                var type = Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor");
                _clearGCMethodInfo = type.GetMethod("ClearGlobalCache", BindingFlags.NonPublic | BindingFlags.Static);
            }

            _clearGCMethodInfo.Invoke(null, null);
        }

        #endregion
    }
}