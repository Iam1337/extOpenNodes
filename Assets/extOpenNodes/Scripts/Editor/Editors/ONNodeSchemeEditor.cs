/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;
using UnityEditorInternal;

using System;

using extOpenNodes.Core;

namespace extOpenNodes.Editor.Editors
{
    [CustomEditor(typeof(ONNodeScheme), true)]
    public class ONNodeSchemeEditor : UnityEditor.Editor
    {
        #region Private Static Vars

        private static readonly GUIContent _workflowContent = new GUIContent("Node Scheme:");

        private static readonly GUIContent _nameContent = new GUIContent("Name:");

        private static readonly GUIContent _selfOutputContent = new GUIContent("Self Output:");

        private static readonly GUIContent _inputPropertySchemesContent = new GUIContent("Input Properties Schemes:");

        private static readonly GUIContent _outputPropertySchemesContent = new GUIContent("Output Properties Schemes:");

        #endregion

        #region Public Vars

        #endregion

        #region Protected Vars

        #endregion

        #region Private Vars

        private ONNodeScheme _target;

        private SerializedProperty _nameProperty;

        private SerializedProperty _selfOutputProperty;

        private SerializedProperty _inputPropertiesSchemesProperty;

        private SerializedProperty _outputPropertiesSchemesProperty;

        private ReorderableList _inputReordableList;

        private ReorderableList _outputReordableList;

        #endregion

        #region Unity Methods

        protected void OnEnable()
        {
            _target = target as ONNodeScheme;

            _nameProperty = serializedObject.FindProperty("schemeName");
            _selfOutputProperty = serializedObject.FindProperty("selfOutput");
            _inputPropertiesSchemesProperty = serializedObject.FindProperty("inputPropertiesSchemes");
            _outputPropertiesSchemesProperty = serializedObject.FindProperty("outputPropertiesSchemes");

            _inputReordableList = new ReorderableList(serializedObject, _inputPropertiesSchemesProperty, true, true, true, true);
            _inputReordableList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3;
            _inputReordableList.drawHeaderCallback += (rect) => { GUI.Label(rect, _inputPropertySchemesContent); };
            _inputReordableList.drawElementCallback = DrawInputElementCallback;
            _inputReordableList.onAddCallback = OnInputAddCallback;

            _outputReordableList = new ReorderableList(serializedObject, _outputPropertiesSchemesProperty, true, true, true, true);
            _outputReordableList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3;
            _outputReordableList.drawHeaderCallback += (rect) => { GUI.Label(rect, _outputPropertySchemesContent); };
            _outputReordableList.drawElementCallback = (rect, index, isActive, isFocused) => { EditorGUI.PropertyField(rect, _inputPropertiesSchemesProperty.GetArrayElementAtIndex(index), GUIContent.none);};
            _outputReordableList.drawElementCallback = DrawOutputElementCallback;
            _outputReordableList.onAddCallback = OnOutputAddCallback;
        }

        #endregion

        #region Public Methods

        public override void OnInspectorGUI()
        {
            var defaultColor = GUI.color;

            serializedObject.Update();

            // LOGO
            GUILayout.Space(10);
            ONEditorLayout.Logo();
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();

            // INSPECTOR
            GUILayout.Label(_workflowContent, EditorStyles.boldLabel);
            GUILayout.BeginVertical("box");

            // SCHEME NAME
            GUI.color = IsNameAvaible() ? defaultColor : Color.red;
            EditorGUILayout.PropertyField(_nameProperty, _nameContent);
            GUI.color = defaultColor;

            // SELF OUTPUT NAME
            EditorGUILayout.PropertyField(_selfOutputProperty, _selfOutputContent);

            GUILayout.EndVertical();

            // INPUT PROPERTIES
            GUILayout.Label(_inputPropertySchemesContent, EditorStyles.boldLabel);
            GUILayout.BeginVertical("box");

            _inputReordableList.DoLayoutList();

            GUILayout.EndVertical();

            // OUTPUT PROPERTIES
            GUILayout.Label(_outputPropertySchemesContent, EditorStyles.boldLabel);
            GUILayout.BeginVertical("box");

            _outputReordableList.DoLayoutList();

            GUILayout.EndVertical();

            var change = EditorGUI.EndChangeCheck();
            if (change) serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private bool IsNameAvaible()
        {
            var schemes = ONNodesUtils.GetSchemes(_target.TargetType);
            foreach (var scheme in schemes)
            {
                if (scheme == _target)
                    continue;

                if (scheme.Name.Equals(_target.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnInputAddCallback(ReorderableList list)
        {
            OnAddCallback(_inputPropertiesSchemesProperty, list, ONPropertyType.Input);
        }

        private void OnOutputAddCallback(ReorderableList list)
        {
            OnAddCallback(_outputPropertiesSchemesProperty, list, ONPropertyType.Output);
        }

        private void OnAddCallback(SerializedProperty property, ReorderableList list, ONPropertyType propertyType)
        {
            property.InsertArrayElementAtIndex(property.arraySize);

            var newProperty = property.GetArrayElementAtIndex(property.arraySize - 1);

            newProperty.FindPropertyRelative("typeName").stringValue = _target.TargetType.AssemblyQualifiedName;
            newProperty.FindPropertyRelative("name").stringValue = "Property";
            newProperty.FindPropertyRelative("propertyType").enumValueIndex = (int)propertyType;
        }

        private void DrawInputElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawElementCallback(_inputPropertiesSchemesProperty, rect, index, isActive, isFocused);
        }

        private void DrawOutputElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawElementCallback(_outputPropertiesSchemesProperty, rect, index, isActive, isFocused);
        }

        private void DrawElementCallback(SerializedProperty property, Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 1;

            EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), GUIContent.none);
        }

        #endregion
    }
}