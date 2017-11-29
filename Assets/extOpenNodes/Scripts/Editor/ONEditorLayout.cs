/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;

using extOpenNodes.Core;
using System.Reflection;

namespace extOpenNodes.Editor
{
    public static class ONEditorLayout
    {
        #region Static Private Vars

        private static readonly GUIContent _noneContent = new GUIContent("- None - ");

        private static readonly GUIContent _inputsContent = new GUIContent("Inputs:");

        private static readonly GUIContent _outputsContent = new GUIContent("Outputs:");

        #endregion

        #region Static Public Methods

        public static void Logo()
        {
            if (ONEditorTextures.IronWall != null)
            {
                EditorGUILayout.Space();

                var rect = GUILayoutUtility.GetRect(0, 0);
                var width = ONEditorTextures.IronWall.width * 0.2f;
                var height = ONEditorTextures.IronWall.height * 0.2f;

                rect.x = rect.width * 0.5f - width * 0.5f;
                rect.y = rect.y + rect.height * 0.5f - height * 0.5f;
                rect.width = width;
                rect.height = height;

                GUI.DrawTexture(rect, ONEditorTextures.IronWall);
                EditorGUILayout.Space();
            }
        }

        public static void Grid(Rect container, Vector2 offset)
        {
            var baseOffset = offset;

            var gridScale = 15f;
            var xLinesCount = Mathf.CeilToInt(container.width / gridScale) + 2;
            var yLinesCount = Mathf.CeilToInt(container.height / gridScale) + 2;

            var pseudoXPosition = Mathf.FloorToInt((offset.x / gridScale) % 5);
            var pseudoYPosition = Mathf.FloorToInt((offset.y / gridScale) % 5);

            offset = new Vector2(offset.x % gridScale, offset.y % gridScale);

            if (pseudoXPosition < 0)
                pseudoXPosition += 5;
            if (pseudoYPosition < 0)
                pseudoYPosition += 5;

            if (offset.x < 0)
                offset.x += gridScale;
            if (offset.y < 0)
                offset.y += gridScale;

            var rulePosition = new Rect();

            for (var x = 0; x < xLinesCount; x++)
            {
                var linePosition = container.x + (x * gridScale) + offset.x;
                var pseudoPosition = Mathf.CeilToInt(x - pseudoXPosition);

                Handles.color = new Color(0, 0, 0, pseudoPosition % 5 == 0 ? 0.2f : 0.1f);
                Handles.DrawLine(new Vector3(linePosition, container.y, 0), new Vector3(linePosition, container.y + container.height, 0));

                if (pseudoPosition % 5 == 0)
                {
                    var ruleValue = (int)((linePosition - baseOffset.x) / gridScale);
                    var ruleContent = new GUIContent(ruleValue.ToString());
                    var ruleSize = ONEditorStyles.GridRuleX.CalcSize(ruleContent);

                    rulePosition.size = ruleSize;
                    rulePosition.center = new Vector2(linePosition, container.y + ruleSize.y / 2f);
                    GUI.Label(rulePosition, ruleContent, ONEditorStyles.GridRuleX);
                    rulePosition.center = new Vector2(linePosition, container.y + container.height - ruleSize.y / 2f);
                    GUI.Label(rulePosition, ruleContent, ONEditorStyles.GridRuleX);
                }
            }

            for (var y = 0; y < yLinesCount; y++)
            {
                var linePosition = container.y + (y * gridScale) + offset.y;
                var pseudoPosition = Mathf.CeilToInt(y - pseudoYPosition);

                Handles.color = new Color(0, 0, 0, pseudoPosition % 5 == 0 ? 0.2f : 0.1f);
                Handles.DrawLine(new Vector3(container.x, linePosition, 0), new Vector3(container.x + container.width, linePosition, 0));

                if (pseudoPosition % 5 == 0)
                {
                    var ruleValue = (int)((linePosition - baseOffset.y) / gridScale);
                    var ruleContent = new GUIContent(ruleValue.ToString());
                    var ruleSize = ONEditorStyles.GridRuleY.CalcSize(ruleContent);

                    rulePosition.size = ruleSize;
                    rulePosition.center = new Vector2(container.x + ruleSize.x, linePosition);
                    GUI.Label(rulePosition, ruleContent, ONEditorStyles.GridRuleY);
                    rulePosition.center = new Vector2(container.x + container.width - ruleSize.x, linePosition);
                    GUI.Label(rulePosition, ruleContent, ONEditorStyles.GridRuleY);
                }
            }

            Handles.color = Color.white;
        }

        public static void DrawIOHeader(ONNode node)
        {
            EditorGUILayout.BeginHorizontal();
            DrawProperties(node.InputProperties, _inputsContent, ONEditorStyles.InputProperty);
            GUILayout.FlexibleSpace();
            DrawProperties(node.OutputProperties, _outputsContent, ONEditorStyles.OutputProperty);
            EditorGUILayout.EndHorizontal();
        }

        public static ONNodeSchema SchemesPopup(Type componentType, ONNodeSchema schema, GUIContent content)
        {
            var schemes = ONNodesUtils.GetNodeSchemes(componentType);
            var popupContent = new List<GUIContent>();
            var currentIndex = 0;

            if (schemes.Length > 0)
            {
                for (var i = 0; i < schemes.Length; i++)
                {
                    if (schema == schemes[i])
                    {
                        currentIndex = i;
                    }

                    popupContent.Add(new GUIContent(schemes[i].Name));
                }
            }
            else
            {
                popupContent.Add(_noneContent);
            }

            currentIndex = EditorGUILayout.Popup(content, currentIndex, popupContent.ToArray());

            return schemes.Length == 0 ? null : schemes[currentIndex];
        }

        public static string InputMembersPopup(Rect rect, Type targetType, string memberName, GUIContent content)
        {
            return MembersPopup(rect, memberName, ONEditorUtils.GetInputMembersInfos(targetType), content);
        }

        public static string OutputMembersPopup(Rect rect, Type targetType, string memberName, GUIContent content)
        {
            return MembersPopup(rect, memberName, ONEditorUtils.GetOutputMembersInfos(targetType), content);
        }
        
        #endregion

        #region Static Private Methods

        private static string MembersPopup(Rect rect, string memberName, List<MemberInfo> membersInfos, GUIContent content)
        {
            var popupContent = new List<GUIContent>();
            var currentIndex = 0;

            if (membersInfos.Count > 0)
            {
                for (var i = 0; i < membersInfos.Count; i++)
                {
                    if (membersInfos[i].Name == memberName)
                    {
                        currentIndex = i;
                    }

                    popupContent.Add(new GUIContent(ONEditorUtils.MemberFrendlyName(membersInfos[i])));
                }
            }
            else
            {
                popupContent.Add(_noneContent);
            }

            currentIndex = EditorGUI.Popup(rect, content, currentIndex, popupContent.ToArray());

            return membersInfos.Count == 0 ? "-none-" : membersInfos[currentIndex].Name;
        }


        private static void DrawProperties(List<ONProperty> properties, GUIContent title, GUIStyle style)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label(title, ONEditorStyles.CenterBoldLabel);

            if (properties.Count == 0)
            {
                GUILayout.Label("- None -", style);
            }
            foreach (var property in properties)
            {
                GUILayout.Label(property.Name, style);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}