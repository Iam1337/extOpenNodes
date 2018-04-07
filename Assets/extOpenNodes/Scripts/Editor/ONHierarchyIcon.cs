/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System;

namespace extOpenNodes.Editor
{
	[InitializeOnLoad]
	public static class ONHierarchyIcon
	{
		#region Constructor Methods

		static ONHierarchyIcon()
		{
			EditorApplication.hierarchyWindowItemOnGUI =
				(EditorApplication.HierarchyWindowItemCallback)
					Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI,
						(EditorApplication.HierarchyWindowItemCallback)DrawHierarchyIcon);
		}

		#endregion

		#region Private Methods

		private static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
		{
			if (ONEditorTextures.IronWall == null) return;

			var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (gameObject == null) return;

			var workflow = gameObject.GetComponent<ONWorkflow>();
			if (workflow == null) return;

			var rect = new Rect(selectionRect.x + selectionRect.width - 18f, selectionRect.y, 16f, 16f);
			GUI.DrawTexture(rect, ONEditorTextures.IronWall);
		}

		#endregion
	}
}