/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

namespace extOpenNodes.Editor
{
	public static class ONEditorSettings
	{
		#region Static Public Vars

		public static string Workflow
		{
			get { return _workflowRoot; }
		}

		#endregion

		#region Static Private Vars

		private const string _settingRoot = "extOpenNodes.";

		private const string _workflowRoot = _settingRoot + "workflow.";

		#endregion

		#region Static Public Methods

		// FLOAT
		public static void SetFloat(string settingPath, float value)
		{
			EditorPrefs.SetFloat(settingPath + ".float", value);
		}

		public static float GetFloat(string settingPath, float defaultSetting)
		{
			return EditorPrefs.GetFloat(settingPath + ".float", defaultSetting);
		}

		// BOOL
		public static void SetBool(string settingPath, bool value)
		{
			EditorPrefs.SetBool(settingPath + ".bool", value);
		}

		public static bool GetBool(string settingPath, bool defaultSetting)
		{
			return EditorPrefs.GetBool(settingPath + ".bool", defaultSetting);
		}

		// INT
		public static void SetInt(string settingPath, int value)
		{
			EditorPrefs.SetInt(settingPath + ".int", value);
		}

		public static int GetInt(string settingPath, int defaultSetting)
		{
			return EditorPrefs.GetInt(settingPath + ".int", defaultSetting);
		}

		// STRING
		public static void SetString(string settingPath, string value)
		{
			EditorPrefs.SetString(settingPath + ".string", value);
		}

		public static string GetString(string settingPath, string defaultSetting)
		{
			return EditorPrefs.GetString(settingPath + ".string", defaultSetting);
		}

		// COLOR
		public static void SetColor(string settingPath, Color color)
		{
			EditorPrefs.SetFloat(settingPath + ".r", color.r);
			EditorPrefs.SetFloat(settingPath + ".g", color.g);
			EditorPrefs.SetFloat(settingPath + ".b", color.b);
			EditorPrefs.SetFloat(settingPath + ".a", color.a);
		}

		public static Color GetColor(string settingPath, Color defaultColor)
		{
			var keyR = settingPath + ".r";
			var keyG = settingPath + ".g";
			var keyB = settingPath + ".b";
			var keyA = settingPath + ".a";

			if (!EditorPrefs.HasKey(keyR) || !EditorPrefs.HasKey(keyG) ||
				!EditorPrefs.HasKey(keyB) || !EditorPrefs.HasKey(keyA))
			{
				return defaultColor;
			}

			var color = new Color();

			color.r = EditorPrefs.GetFloat(keyR, 1);
			color.g = EditorPrefs.GetFloat(keyG + ".g", 1);
			color.b = EditorPrefs.GetFloat(keyB + ".b", 1);
			color.a = EditorPrefs.GetFloat(keyA + ".a", 1);

			return color;
		}

		// VECTOR2
		public static void SetVector2(string settingsPath, Vector2 vector)
		{
			EditorPrefs.SetFloat(settingsPath + ".x", vector.x);
			EditorPrefs.SetFloat(settingsPath + ".y", vector.y);
		}

		public static Vector2 GetVector2(string settingsPath, Vector2 defaultVector)
		{
			var keyX = settingsPath + ".x";
			var keyY = settingsPath + ".y";

			if (!EditorPrefs.HasKey(keyX) || !EditorPrefs.HasKey(keyY))
			{
				return defaultVector;
			}

			var vector = new Vector2();

			vector.x = EditorPrefs.GetFloat(keyX, 0);
			vector.y = EditorPrefs.GetFloat(keyY, 0);

			return vector;
		}

		#endregion
	}
}