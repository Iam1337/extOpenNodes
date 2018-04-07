/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using System;

using extOpenNodes.Core;
using UnityEditor;

namespace extOpenNodes.Editor
{
	[Serializable]
	public class ONPropertyScheme
	{
		#region Public Vars

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public string Member
		{
			get { return _member; }
			set { _member = value; }
		}

		public int SortIndex
		{
			get { return _sortIndex; }
			set { _sortIndex = value; }
		}

		public Type TargetType
		{
			get
			{
				if (_type == null)
				{
					if (_monoScript != null)
						_type = _monoScript.GetClass();
					else
						_type = Type.GetType(_typeName);
				}

				return _type;
			}
			set
			{
				_type = value;
				_typeName = value.AssemblyQualifiedName;
				_monoScript = ONEditorUtils.GetTypeScript(value);
			}
		}

		public ONPropertyType PropertyType
		{
			get { return _propertyType; }
			set { _propertyType = value; }
		}

		#endregion

		#region Private Vars 

		[SerializeField]
		private string _name;

		[SerializeField]
		private string _member;

		[SerializeField]
		private string _typeName;

		[SerializeField]
		private MonoScript _monoScript;

		[SerializeField]
		private ONPropertyType _propertyType;

		[NonSerialized]
		private int _sortIndex;

		[NonSerialized]
		private Type _type;

		#endregion
	}
}