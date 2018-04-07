/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

namespace extOpenNodes.Nodes
{
	[ONNode("Examples/Reference")]
	public class ONExampleReference : MonoBehaviour
	{
		#region Public Methods

		[ONInput(0, "Reference")]
		public Component Reference;

		#endregion
	}
}