/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;
using UnityEngine.Events;

using System;

using extOpenNodes.Core;

namespace extOpenNodes.Nodes
{
	[Serializable]
	public class Event1 : UnityEvent<int>
	{ }

	[Serializable]
	public class Event2 : UnityEvent<int, int>
	{ }

	[ONNode("Examples/Events")]
	public class ONExampleEvents : MonoBehaviour
	{
		#region Public Methods

		[ONInput(0, "Event 1")]
		public Event1 Ev1;

		[ONInput(1, "Event 2")]
		public Event2 Ev2;

		#endregion
	}
}