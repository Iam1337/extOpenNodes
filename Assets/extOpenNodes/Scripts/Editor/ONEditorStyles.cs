/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

namespace extOpenNodes.Editor
{
    public static class ONEditorStyles
    {
        #region Static Private Vars

        private static GUIStyle _viewBackground;

        private static GUIStyle _viewBorder;

        private static GUIStyle _inputProperty;

        private static GUIStyle _outputProperty;

        private static GUIStyle _centerLabel;

        private static GUIStyle _centerBoldLabel;

        private static GUIStyle _gridRuleX;

        private static GUIStyle _gridRuleY;

        #endregion

        #region Static Public Vars

        public static GUIStyle CenterLabel
        {
            get
            {
                if (_centerLabel == null)
                {
                    _centerLabel = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return _centerLabel;
            }
        }

        public static GUIStyle CenterBoldLabel
        {
            get
            {
                if (_centerBoldLabel == null)
                {
                    _centerBoldLabel = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    };
                }

                return _centerBoldLabel;
            }
        }

        public static GUIStyle GridRuleX
        {
            get 
            {
                if (_gridRuleX == null)
                {
                    _gridRuleX = new GUIStyle(EditorStyles.label);
                    _gridRuleX.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    _gridRuleX.fontSize = 10;
                    _gridRuleX.alignment = TextAnchor.MiddleCenter;
                }

                return _gridRuleX;
            }
        }

        public static GUIStyle GridRuleY
        {
            get
            {
                if (_gridRuleY == null)
                {
                    _gridRuleY = new GUIStyle(GridRuleX);
                    _gridRuleY.alignment = TextAnchor.MiddleRight;
                }

                return _gridRuleY;
            }
        }

        public static GUIStyle ViewBackground
        {
            get
            {
                if (_viewBackground == null)
                {
                    _viewBackground = new GUIStyle();
                    _viewBackground.normal.background = ONEditorTextures.ViewBackground;
                    _viewBackground.border = new RectOffset(7, 7, 7, 7);
                }

                return _viewBackground;
            }
        }

        public static GUIStyle ViewBorder
        {
            get
            {
                if (_viewBorder == null)
                {
                    _viewBorder = new GUIStyle();
                    _viewBorder.normal.background = ONEditorTextures.ViewBorder;
                    _viewBorder.border = new RectOffset(7, 7, 7, 7);
                }

                return _viewBorder;
            }
        }

        public static GUIStyle InputProperty
        {
            get
            {
                if (_inputProperty == null)
                {
                    _inputProperty = new GUIStyle("label");
                    _inputProperty.alignment = TextAnchor.UpperLeft;
                }
                return _inputProperty;
            }
        }

        public static GUIStyle OutputProperty
        {
            get
            {
                if (_outputProperty == null)
                {
                    _outputProperty = new GUIStyle("label");
                    _outputProperty.alignment = TextAnchor.UpperRight;
                }
                return _outputProperty;
            }
        }

        #endregion
    }
}