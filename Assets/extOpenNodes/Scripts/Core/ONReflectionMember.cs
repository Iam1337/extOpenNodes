/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Reflection;

namespace extOpenNodes.Core
{
	[Serializable]
	public class ONReflectionMember
	{
		#region Extensions

		private enum MemberType
		{
			Field,

			Property,

			Method
		}

		#endregion

		#region Public Vars

		public Component Target
		{
			get
			{
				if (!_init) Init();
				return target;
			}
			set
			{
				target = value;
				Clear();
			}
		}

		public string Member
		{
			get
			{
				if (!_init) Init();
				return member;
			}
			set
			{
				member = value;
				Clear();
			}
		}

		public Type ValueType
		{
			get
			{
				if (!_init) Init();
				return _valueType;
			}
		}

		public bool CanRead
		{
			get
			{
				if (!_init) Init();
				return _canRead;
			}
		}

		public bool CanWrite
		{
			get
			{
				if (!_init) Init();
				return _canWrite;
			}
		}

		public MemberInfo MemberInfo
		{
			get
			{
				if (!_init) Init();

				if (_memberType == MemberType.Field)
					return _fieldInfo;
				if (_memberType == MemberType.Property)
					return _propertyInfo;
				if (_memberType == MemberType.Method)
					return _methodInfo;

				return null;
			}
		}

		#endregion

		#region Protected Vars

		[SerializeField]
		protected Component target;

		[SerializeField]
		protected string member;

		#endregion

		#region Private Vars

		[NonSerialized]
		private bool _init;

		[NonSerialized]
		private bool _canRead;

		[NonSerialized]
		private bool _canWrite;

		[NonSerialized]
		private Type _valueType;

		[NonSerialized]
		private MemberType _memberType;

		[NonSerialized]
		private FieldInfo _fieldInfo;

		[NonSerialized]
		private PropertyInfo _propertyInfo;

		[NonSerialized]
		private MethodInfo _methodInfo;

		#endregion

		#region Public Methods

		public bool IsValid()
		{
			if (!_init) Init();
			return _init;
		}

		public bool IsMethod()
		{
			if (!_init) Init();
			return _memberType == MemberType.Method;
		}

		public object GetValue()
		{
			if (!_init) Init();
			if (!_canRead) return null;

			if (_memberType == MemberType.Field)
			{
				return _fieldInfo.GetValue(target);
			}
			if (_memberType == MemberType.Property && _propertyInfo.CanRead)
			{
				return _propertyInfo.GetValue(target, null);
			}

			return null;
		}

		public void SetValue(object value)
		{
			if (!_init) Init();
			if (!_canWrite) return;

			value = ConvertValue(value);

			if (_memberType == MemberType.Field)
			{
				_fieldInfo.SetValue(target, value);
			}
			else if (_memberType == MemberType.Property)
			{
				_propertyInfo.SetValue(target, value, null);
			}
		}

		public Delegate CreateDelegate(Type delegateType)
		{
			if (!_init) Init();

			if (_memberType == MemberType.Method)
			{
				return Delegate.CreateDelegate(delegateType, target, _methodInfo, false);
			}

			return null;
		}

		public object Invoke(params object[] values)
		{
			if (!_init) Init();

			if (_memberType == MemberType.Method)
			{
				return _methodInfo.Invoke(target, values);
			}

			return null;
		}

		#endregion

		#region Private Methods

		private object ConvertValue(object value)
		{
			if (value == null || _valueType == null || _valueType.IsAssignableFrom(value.GetType()))
			{
				return value;
			}

			return Convert.ChangeType(value, _valueType);
		}

		private void Clear()
		{
			_init = false;
			_valueType = null;
			_fieldInfo = null;
			_propertyInfo = null;
			_canRead = false;
			_canWrite = false;
		}

		private void Init()
		{
			var targetType = target.GetType();
			var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			var membersInfos = targetType.GetMember(member, bindingFlags);

			foreach (var memberInfo in membersInfos)
			{
				var fieldInfo = memberInfo as FieldInfo;
				if (fieldInfo != null)
				{
					_fieldInfo = fieldInfo;
					_valueType = fieldInfo.FieldType;
					_memberType = MemberType.Field;
					_canRead = true;
					_canWrite = true;
					_init = true;

					return;
				}

				var propertyInfo = memberInfo as PropertyInfo;
				if (propertyInfo != null)
				{
					_propertyInfo = propertyInfo;
					_valueType = propertyInfo.PropertyType;
					_memberType = MemberType.Property;
					_canRead = propertyInfo.CanRead;
					_canWrite = propertyInfo.CanWrite;
					_init = true;

					return;
				}

				var methodInfo = memberInfo as MethodInfo;
				if (methodInfo != null)
				{
					_methodInfo = methodInfo;
					_valueType = null;
					_memberType = MemberType.Method;
					_canRead = false;
					_canWrite = false;
					_init = true;

					return;
				}
			}
		}

		#endregion
	}
}