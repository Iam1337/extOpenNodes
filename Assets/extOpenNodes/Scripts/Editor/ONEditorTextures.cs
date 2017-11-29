/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

namespace extOpenNodes.Editor
{
    public static class ONEditorTextures
    {
        #region Static Private Vars

        private const string _defaultFolder = "extOpenNodes/";

        private static Texture2D _iwIcon;

        private static Texture2D _viewBackgroundTexture;

        private static Texture2D _viewBorderTexture;

        private static Texture2D _propertyTexture;

        private static Texture2D _linkTexture;

        private static bool _isProSkin;

        #endregion

        #region Static Public Vars

        public static Texture2D IronWall
        {
            get
            {
                if (_iwIcon == null || EditorGUIUtility.isProSkin != _isProSkin)
                {
                    _isProSkin = EditorGUIUtility.isProSkin;

                    if (_iwIcon != null)
                    {
                        Resources.UnloadAsset(_iwIcon);
                    }

                    _iwIcon = LoadTexture(_isProSkin ? "IW_logo_light" : "IW_logo_dark");
                }

                return _iwIcon;
            }
        }

        public static Texture2D ViewBackground
        {
            get
            {
                if (_viewBackgroundTexture == null || EditorGUIUtility.isProSkin != _isProSkin)
                {
                    _isProSkin = EditorGUIUtility.isProSkin;

                    if (_viewBackgroundTexture != null)
                    {
                        Resources.UnloadAsset(_viewBackgroundTexture);
                    }

                    //TODO: Make dark skin texture.
                    _viewBackgroundTexture = LoadTexture(_isProSkin ? "ON_viewbackground" : "ON_viewbackground");
                }

                return _viewBackgroundTexture;
            }
        }

        public static Texture2D ViewBorder
        {
            get
            {
                if (_viewBorderTexture == null || EditorGUIUtility.isProSkin != _isProSkin)
                {
                    _isProSkin = EditorGUIUtility.isProSkin;

                    if (_viewBorderTexture != null)
                    {
                        Resources.UnloadAsset(_viewBorderTexture);
                    }

                    //TODO: Make dark skin texture.
                    _viewBorderTexture = LoadTexture(_isProSkin ? "ON_viewborder" : "ON_viewborder");
                }

                return _viewBorderTexture;
            }
        }

        public static Texture2D PropertyTexture
        {
            get
            {
                if (_propertyTexture == null || EditorGUIUtility.isProSkin != _isProSkin)
                {
                    _isProSkin = EditorGUIUtility.isProSkin;

                    if (_propertyTexture != null)
                    {
                        Resources.UnloadAsset(_propertyTexture);
                    }

                    _propertyTexture = LoadTexture(_isProSkin ? "ON_property_dark" : "ON_property_light");
                }

                return _propertyTexture;
            }
        }

        public static Texture2D LinkTexture
        {
            get
            {
                if (_linkTexture == null || EditorGUIUtility.isProSkin != _isProSkin)
                {
                    _isProSkin = EditorGUIUtility.isProSkin;

                    if (_linkTexture != null)
                    {
                        Resources.UnloadAsset(_linkTexture);
                    }

                    //TODO: Make dark skin texture.
                    _linkTexture = LoadTexture(_isProSkin ? "ON_link" : "ON_link");
                }

                return _linkTexture;
            }
        }

        #endregion

        #region Static Private Methods

        private static Texture2D LoadTexture(string fileName)
        {
            return Resources.Load<Texture2D>(_defaultFolder + fileName);
        }

        #endregion
    }
}