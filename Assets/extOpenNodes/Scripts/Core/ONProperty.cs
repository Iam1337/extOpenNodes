/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using System;

namespace extOpenNodes.Core
{
	[Serializable]
	public class ONProperty : ONElement
	{
		#region Public Vars

		public ONPropertyType PropertyType
		{
			get { return _propertyType; }
			set { _propertyType = value; }
		}

		public Type ValueType
		{
			get
			{
				if (_targetCast && Node != null && Node.Target != null)
				{
					return Node.Target.GetType();
				}

				if (_reflectionMember != null &&
					_reflectionMember.IsValid())
				{
					return _reflectionMember.ValueType;
				}

				return null;
			}
		}

		public ONNode Node
		{
			get
			{
				if (_node == null && Workflow != null)
					_node = Workflow.GetElement(_nodeId) as ONNode;

				return _node;
			}
			set
			{
				_node = value;
				_nodeId = _node.ElementId;
			}
		}

		public bool TargetCast
		{
			get { return _targetCast; }
			set { _targetCast = value; }
		}

		public int SortIndex
		{
			get { return _index; }
			set { _index = value; }
		}

		public ONReflectionMember ReflectionMember
		{
			get { return _reflectionMember; }
			set { _reflectionMember = value; }
		}

		#endregion

		#region Private Vars

		[SerializeField]
		private int _index;

		[SerializeField]
		private string _nodeId;

		[SerializeField]
		private bool _targetCast = false;

		[SerializeField]
		private ONPropertyType _propertyType;

		[SerializeField]
		private ONReflectionMember _reflectionMember;

		[NonSerialized]
		private ONNode _node;

		#endregion

		#region Public Methods

		public void SetValue(object value)
		{
			if (_targetCast)
				return;

			if (_reflectionMember != null &&
				_reflectionMember.IsValid() &&
				_reflectionMember.CanWrite)
			{
				_reflectionMember.SetValue(value);
			}
		}

		public object GetValue()
		{
			if (_targetCast)
				return Node.Target;

			if (_reflectionMember != null &&
				_reflectionMember.IsValid() &&
				_reflectionMember.CanRead)
			{
				return _reflectionMember.GetValue();
			}

			return null;
		}

		#endregion
	}
}