/* Copyright (c) 2018 ExT (V.Sigalkin) */

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace extOpenNodes.Editor
{
	public static class ONEditorUtils
	{
		#region Static Private Methods

		private static MethodInfo _clearGCMethodInfo;

		#endregion

		#region Static Public Methods

		public static void ClearGlobalCache()
		{
			if (_clearGCMethodInfo == null)
			{
				var type = Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor");
				_clearGCMethodInfo = type.GetMethod("ClearGlobalCache", BindingFlags.NonPublic | BindingFlags.Static);
			}

			_clearGCMethodInfo.Invoke(null, null);
		}

		public static string TypeFriendlyName(Type type)
		{
			if (type == typeof(byte))
				return "byte";
			if (type == typeof(bool))
				return "bool";
			if (type == typeof(int))
				return "int";
			if (type == typeof(string))
				return "string";
			if (type == typeof(short))
				return "short";
			if (type == typeof(long))
				return "long";
			if (type == typeof(float))
				return "float";
			if (type == typeof(double))
				return "double";
			if (type == typeof(decimal))
				return "decimal";
			if (type.IsGenericType)
			{
				var typeName = type.Name.Split('`')[0];

				var genericArguments = type.GetGenericArguments().Select(t => TypeFriendlyName(t)).ToArray();
				var genericString = "<" + string.Join(", ", genericArguments) + ">";

				return typeName + genericString;
			}

			return type.Name;
		}

		public static string MemberFrendlyName(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo)
			{
				var fieldInfo = (FieldInfo)memberInfo;

				return string.Format("[F] {0} {1}", TypeFriendlyName(fieldInfo.FieldType), fieldInfo.Name);
			}
			if (memberInfo is PropertyInfo)
			{
				var propertyInfo = (PropertyInfo)memberInfo;
				var propertyString = string.Empty;

				if (propertyInfo.CanRead)
					propertyString += "get;";

				if (propertyInfo.CanWrite)
					propertyString += "set;";

				return string.Format("[P] {0} {1} {{{2}}}", TypeFriendlyName(propertyInfo.PropertyType), propertyInfo.Name, propertyString);
			}
			if (memberInfo is MethodInfo)
			{
				var methodInfo = (MethodInfo)memberInfo;
				var parameters = methodInfo.GetParameters();
				var parametersString = string.Empty;

				foreach (var parameter in parameters)
				{
					parametersString += TypeFriendlyName(parameter.ParameterType) + " " + parameter.Name + ", ";
				}

				if (parametersString.Length > 2)
					parametersString = parametersString.Remove(parametersString.Length - 2);

				return string.Format("[M] {0}({1})", methodInfo.Name, parametersString);
			}

			return memberInfo.Name;
		}

		public static List<MemberInfo> GetInputMembersInfos(Type targetType)
		{
			var members = new List<MemberInfo>();

			var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			var membersInfos = targetType.GetMembers(bindingFlags);

			foreach (var memberInfo in membersInfos)
			{
				if (memberInfo is FieldInfo || memberInfo is PropertyInfo)
				{
					var fieldInfo = memberInfo as FieldInfo;
					if (fieldInfo != null)
					{
						members.Add(fieldInfo);
						continue;
					}

					var propertyInfo = memberInfo as PropertyInfo;
					if (propertyInfo != null && propertyInfo.CanWrite)
					{
						members.Add(propertyInfo);
						continue;
					}
				}
			}

			return members;
		}

		public static List<MemberInfo> GetOutputMembersInfos(Type targetType)
		{
			var members = new List<MemberInfo>();

			var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			var membersInfos = targetType.GetMembers(bindingFlags);

			foreach (var memberInfo in membersInfos)
			{
				if (memberInfo is FieldInfo || memberInfo is PropertyInfo || memberInfo is MethodInfo)
				{
					var methodInfo = memberInfo as MethodInfo;
					if (methodInfo != null && methodInfo.IsSpecialName)
					{
						continue;
					}

					var fieldInfo = memberInfo as FieldInfo;
					if (fieldInfo != null)
					{
						members.Add(fieldInfo);
						continue;
					}

					var propertyInfo = memberInfo as PropertyInfo;
					if (propertyInfo != null && propertyInfo.CanRead)
					{
						members.Add(propertyInfo);
						continue;
					}

					if (methodInfo != null && methodInfo.ReturnType == typeof(void))
					{
						members.Add(methodInfo);
					}
				}
			}

			return members;
		}

		public static MonoScript GetTypeScript(Type type)
		{
			var guids = AssetDatabase.FindAssets("t:" + typeof(MonoScript).Name);

			foreach (var guid in guids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);

				var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
				if (monoScript == null) continue;

				var componentType = monoScript.GetClass();
				if (componentType == null) continue;

				if (componentType == type) return monoScript;
			}

			return null;
		}

		#endregion
	}
}