using UnityEngine;

using extOpenNodes.Core;

namespace extOpenNodes.Nodes
{
	[ONNode("Examples/Methods", false)]
	public class ONExampleMethods : MonoBehaviour
	{
		#region Public Methods

		[ONOutput(0, "Method 1")]
		public void Method1(int value)
		{
			Debug.Log(value);
		}

		[ONOutput(1, "Method 2")]
		public void Method2(int value1, int value2)
		{
			Debug.Log(value1 + ", " + value2);
		}

		#endregion
	}
}