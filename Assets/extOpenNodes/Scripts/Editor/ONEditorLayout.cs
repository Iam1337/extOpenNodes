/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using extOpenNodes.Core;

namespace extOpenNodes.Editor
{
    public static class ONEditorLayout
    {
        #region Static Private Vars

        private static readonly GUIContent _inputsContent = new GUIContent("Inputs:");

        private static readonly GUIContent _outputsContent = new GUIContent("Outputs:");

        #endregion

        #region Static Public Methods

        public static void DrawGrid(Rect container, Vector2 offset)
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
                    rulePosition.center = new Vector2(container.x + container.width - ruleSize.x, linePosition);
                    GUI.Label(rulePosition, ruleContent, ONEditorStyles.GridRuleY);
                }
            }

            Handles.color = Color.white;
        }

        public static void DrawIOHeader(ONNode node)
        {
            EditorGUILayout.BeginHorizontal();
            DrawInputs(node);
            GUILayout.FlexibleSpace();
            DrawOutputs(node);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawInputs(ONNode node)
        {
            var inputProperties = node.InputProperties;

            EditorGUILayout.BeginVertical();
            GUILayout.Label(_inputsContent, ONEditorStyles.CenterBoldLabel);

            if (inputProperties.Count == 0)
            {
                GUILayout.Label("- None -", ONEditorStyles.InputProperty);
            }
            foreach (var property in inputProperties)
            {
                GUILayout.Label(property.Name, ONEditorStyles.InputProperty);
            }

            EditorGUILayout.EndVertical();
        }

        public static void DrawOutputs(ONNode node)
        {
            var outputProperties = node.OutputProperties;

            EditorGUILayout.BeginVertical();
            GUILayout.Label(_outputsContent, ONEditorStyles.CenterBoldLabel);

            if (outputProperties.Count == 0)
            {
                GUILayout.Label("- None -", ONEditorStyles.OutputProperty);
            }
            foreach (var property in outputProperties)
            {
                GUILayout.Label(property.Name, ONEditorStyles.OutputProperty);
            }

            EditorGUILayout.EndVertical();

        }

        #endregion
    }
}