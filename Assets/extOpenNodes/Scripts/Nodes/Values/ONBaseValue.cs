﻿/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

namespace extOpenNodes.Nodes.Values
{
	public abstract class ONBaseValue : MonoBehaviour
	{ }

	public abstract class ONBaseValue<T> : ONBaseValue
	{
		#region Public Vars

		[ONOutput(0, "Value")]
		public T Value { get { return value; } }

		#endregion

		#region Protected Vars

		[SerializeField]
		protected T value;

		#endregion
	}
}