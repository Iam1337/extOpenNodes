/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System;

using extOpenNodes.Core;

namespace extOpenNodes.Editor.Properties
{
    [CustomPropertyDrawer(typeof(ONPropertyScheme))]
    public class ONPropertySchemeProperty : PropertyDrawer
    {
        #region Public Methods

        #endregion

        #region Public Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var color = GUI.color;
            var nameProperty = property.FindPropertyRelative("name");
            var memberProperty = property.FindPropertyRelative("member");
            var typeNameProperty = property.FindPropertyRelative("typeName");
            var targetType = Type.GetType(typeNameProperty.stringValue);
            var propertyTypeProperty = property.FindPropertyRelative("propertyType");
            var propertyType = (ONPropertyType)propertyTypeProperty.enumValueIndex;

            EditorGUI.BeginProperty(rect, label, property);

            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var currentPosition = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(currentPosition, nameProperty, GUIContent.none);

            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (propertyType == ONPropertyType.Input)
            {
                memberProperty.stringValue = ONEditorLayout.InputMembersPopup(currentPosition, targetType, memberProperty.stringValue, GUIContent.none);
            }
            else
            {
                memberProperty.stringValue = ONEditorLayout.OutputMembersPopup(currentPosition, targetType, memberProperty.stringValue, GUIContent.none);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        #endregion

        #region Protected Methods



        #endregion
    }
}