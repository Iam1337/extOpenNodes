/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using extOpenNodes.Nodes.Values;

namespace extOpenNodes.Editor.Editors
{
	[CustomEditor(typeof(ONBaseValue), true)]
	public class ONBaseValueEditor : UnityEditor.Editor
	{
		#region Static Private Vars

		private static GUIContent _valueContent = new GUIContent("Value");

		#endregion

		#region Private Vars

		private SerializedProperty _valueProperty;

		#endregion

		#region Unity Methods

		protected void OnEnable()
		{
			_valueProperty = serializedObject.FindProperty("value");
		}

		#endregion

		#region Public Methods

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();

			GUILayout.Label(_valueContent, ONEditorStyles.CenterBoldLabel);
			EditorGUILayout.PropertyField(_valueProperty, GUIContent.none);

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