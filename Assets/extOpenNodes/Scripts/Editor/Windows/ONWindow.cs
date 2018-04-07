/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System;

using extOpenNodes.Editor.Panels;

namespace extOpenNodes.Editor.Windows
{
	public abstract class ONWindow : EditorWindow
	{
		#region Public Vars

		public abstract ONPanel RootPanel { get; }

		#endregion

		#region Unity Methods

		protected virtual void Awake()
		{ }

		protected abstract void Update();

		protected virtual void OnEnable()
		{
			LoadWindowSettings();
		}

		protected virtual void OnDisable()
		{
			SaveWindowSettings();
		}

		protected virtual void OnDestroy()
		{ }

		protected abstract void OnGUI();

		#endregion

		#region Protected Methods

		protected virtual void LoadWindowSettings()
		{ }

		protected virtual void SaveWindowSettings()
		{ }

		#endregion
	}

	public class ONWindow<TWindow, TPanel> : ONWindow where TWindow : ONWindow where TPanel : ONPanel
	{
		#region Public Vars

		public static TWindow Instance
		{
			get { return GetWindow<TWindow>(false, "", false); }
		}

		public override ONPanel RootPanel
		{
			get { return rootPanel; }
		}

		#endregion

		#region Protected Vars

		protected TPanel rootPanel
		{
			get
			{
				if (_rootPanel == null)
					_rootPanel = CreateRoot();

				return _rootPanel;
			}
		}

		#endregion

		#region Private Vars

		private TPanel _rootPanel;

		#endregion

		#region Unity Methods

		protected override void Update()
		{
			if (rootPanel != null)
				rootPanel.Update();
		}

		protected override void OnGUI()
		{
			DrawRootPanel(new Rect(0, 0, position.width, position.height));
		}

		#endregion

		#region Protected Methods

		protected virtual T CreatePanel<T>(string panelId) where T : ONPanel
		{
			var panel = (T)Activator.CreateInstance(typeof(T), panelId, this);
			if (panel == null) return null;


			return panel;
		}

		protected TPanel CreateRoot()
		{
			if (_rootPanel != null)
			{
				Debug.LogErrorFormat("[{0}] Already has root panel!", GetType());
				return default(TPanel);
			}

			var panel = (TPanel)Activator.CreateInstance(typeof(TPanel), this, "root" + name);

			_rootPanel = panel;

			return panel;
		}

		protected void DrawRootPanel(Rect contentRect)
		{
			if (rootPanel == null) return;

			rootPanel.Rect = contentRect;
			rootPanel.Draw();
		}

		#endregion
	}
}