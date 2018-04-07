/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using extOpenNodes.Core;

namespace extOpenNodes.Nodes.Debugs
{
	[ONNode("Debug/Log", true)]
	public class ONDebugLogNode : MonoBehaviour, IONNodeProcess
	{
		#region Public Vars

		[ONInput(0, "Value")]
		public object Value
		{
			set
			{
				this.value = value;
			}
		}

		#endregion

		#region Protected Vars

		protected object value;

		[SerializeField]
		protected bool debug;

		#endregion

		#region Public Methods

		public void NodeProcess(ONNode node)
		{
			if (value != null && debug)
				Debug.Log("Value: " + value);

			value = null;
		}

		#endregion
	}
}