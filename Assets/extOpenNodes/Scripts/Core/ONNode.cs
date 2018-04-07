/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Collections.Generic;

namespace extOpenNodes.Core
{
	[Serializable]
	public class ONNode : ONElement
	{
		#region Public Vars

		public Component Target
		{
			get { return _target; }
			set { _target = value; }
		}

		public List<ONProperty> InputProperties
		{
			get { return _inputProperties; }
		}

		public List<ONProperty> OutputProperties
		{
			get { return _outputProperties; }
		}

#if UNITY_EDITOR
		// EDITOR ONLY 
		public bool CustomInspector
		{
			get { return _customInspector; }
			set { _customInspector = value; }
		}

		public bool HideInspector
		{
			get { return _hideInspector; }
			set { _hideInspector = value; }
		}
#endif

		#endregion

		#region Private Vars

		[SerializeField]
		private List<ONProperty> _inputProperties = new List<ONProperty>();

		[SerializeField]
		private List<ONProperty> _outputProperties = new List<ONProperty>();

		[SerializeField]
		private Component _target;

#if UNITY_EDITOR
		// EDITOR ONLY 
		[SerializeField]
		private bool _customInspector;

		[SerializeField]
		private bool _hideInspector;
#endif

		#endregion

		#region Public Methods

		public override void Process()
		{
			var targetInterface = _target as IONNodeProcess;

			if (targetInterface != null)
				targetInterface.NodeProcess(this);
		}

		#endregion
	}
}