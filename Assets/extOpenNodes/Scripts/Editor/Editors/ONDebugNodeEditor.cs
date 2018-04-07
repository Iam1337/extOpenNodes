/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using extOpenNodes.Nodes.Debugs;

namespace extOpenNodes.Editor.Editors
{
	[CustomEditor(typeof(ONDebugLogNode), true)]
	public class ONDebugNodeEditor : UnityEditor.Editor
	{
		#region Static Private Vars

		private static GUIContent _valueContent = new GUIContent("Settings");

		#endregion

		#region Private Vars

		private SerializedProperty _debugProperty;

		#endregion

		#region Unity Methods

		protected void OnEnable()
		{
			_debugProperty = serializedObject.FindProperty("debug");
		}

		#endregion

		#region Public Methods

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();

			GUILayout.Label(_valueContent, ONEditorStyles.CenterBoldLabel);
			EditorGUILayout.PropertyField(_debugProperty);

			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}

		#endregion

		#region Protected Methods

		#endregion

		#region Private Methods

		#endregion
	}
}