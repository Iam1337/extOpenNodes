/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Collections.Generic;
//using UnityEditor;
using UnityEditor;

namespace extOpenNodes.Editor
{
	[Serializable]
	public class ONNodeScheme : ScriptableObject
	{
		#region Public Vars

		public string Name
		{
			get { return _schemeName; }
			set { _schemeName = value; }
		}

		public bool SelfOutput
		{
			get { return _selfOutput; }
			set { _selfOutput = value; }
		}

		public List<ONPropertyScheme> InputPropertiesSchemes
		{
			get { return _inputPropertiesSchemes; }
			set { _inputPropertiesSchemes = value; }
		}

		public List<ONPropertyScheme> OutputPropertiesSchemes
		{
			get { return _outputPropertiesSchemes; }
			set { _outputPropertiesSchemes = value; }
		}

		public bool CustomInspector
		{
			get { return _customInspector; }
			set { _customInspector = value; }
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

				foreach (var property in _inputPropertiesSchemes)
				{
					property.TargetType = value;
				}

				foreach (var property in _outputPropertiesSchemes)
				{
					property.TargetType = value;
				}
			}
		}

		#endregion

		#region Private Vars

		[SerializeField]
		private string _schemeName;

		[SerializeField]
		private string _typeName;

		[SerializeField]
		private MonoScript _monoScript;

		[SerializeField]
		private bool _selfOutput;

		[SerializeField]
		private List<ONPropertyScheme> _inputPropertiesSchemes = new List<ONPropertyScheme>();

		[SerializeField]
		private List<ONPropertyScheme> _outputPropertiesSchemes = new List<ONPropertyScheme>();

		[NonSerialized]
		private bool _customInspector;

		[NonSerialized]
		private Type _type;

		#endregion

		#region Private Methods

		#endregion
	}
}