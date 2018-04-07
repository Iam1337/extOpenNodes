/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;
using UnityEngine.Events;

using UnityEditor;

using System;
using System.Reflection;

using extOpenNodes.Core;
using extOpenNodes.Editor.Windows;

namespace extOpenNodes.Editor
{
	public static class ONWorkflowUtils
	{
		#region Static Public Methods

		public static ONWorkflow CreateWorkflow(bool autoOpen)
		{
			var gameObject = new GameObject("ON Workflow");
			var workflow = gameObject.AddComponent<ONWorkflow>();

			Undo.RegisterCreatedObjectUndo(gameObject, "Create Workflow");

			if (autoOpen)
			{
				ONWorkflowWindow.OpenWorkflow(workflow);
			}

			return workflow;
		}

		public static ONNode CreateNode(ONWorkflow workflow, Type componentType)
		{
			var nodesRoot = workflow.NodesRoot;
			if (nodesRoot != null)
			{
				var component = nodesRoot.AddComponent(componentType);
				if (component == null)
				{
					Debug.Log("TODO: Error!");
					return null;
				}

				return CreateNode(workflow, component);
			}

			return null;
		}

		public static ONNode CreateNode(ONWorkflow workflow, Component component)
		{
			var node = workflow.CreateNode(component);

			var componentType = component.GetType();

			var schemes = ONNodesUtils.GetSchemes(componentType);
			if (schemes.Count > 0)
			{
				ONNodesUtils.RebuildNode(workflow, node, schemes[0]);
			}
			else
			{
				node.Name = ONEditorUtils.TypeFriendlyName(componentType);
			}

			if (!node.CustomInspector) node.HideInspector = true;

			return node;
		}

		public static void RemoveNode(ONWorkflow workflow, ONNode node)
		{
			var component = node.Target;
			if (component != null)
			{
				if (component.gameObject == workflow.NodesRoot)
				{
					GameObject.DestroyImmediate(component);
				}
			}

			RemoveLinks(workflow, node);

			workflow.RemoveNode(node);
		}

		public static ONLink CreateLink(ONWorkflow workflow, ONProperty firstProperty, ONProperty secondProperty)
		{
			ONLinkType linkType;
			ONProperty sourceProperty = null;
			ONProperty targetProperty = null;

			if (!CheckPropertyPair(firstProperty, secondProperty, out sourceProperty, out targetProperty))
				return null;

			if (TryLinkProperties(sourceProperty, targetProperty, out linkType))
			{
				if (linkType == ONLinkType.Event && !AddEvent(sourceProperty, targetProperty))
				{
					return null;
				}

				if (linkType == ONLinkType.Value)
				{
					RemoveLinks(workflow, targetProperty);
				}

				var link = workflow.CreateLink(sourceProperty, targetProperty);
				link.LinkType = linkType;

				return link;
			}

			return null;
		}

		public static void RemoveLink(ONWorkflow workflow, ONLink link)
		{
			if (link.LinkType == ONLinkType.Event)
			{
				RemoveEvent(link.SourceProperty, link.TargetProperty);
			}

			workflow.RemoveLink(link);
		}

		public static void RemoveLinks(ONWorkflow worflow, ONNode node)
		{
			var properties = worflow.GetNodeProperties(node);
			foreach (var property in properties)
			{
				RemoveLinks(worflow, property);
			}
		}

		public static void RemoveLinks(ONWorkflow workflow, ONProperty property)
		{
			var links = workflow.GetElementLinks(property);
			foreach (var link in links)
			{
				RemoveLink(workflow, link);
			}
		}

		public static bool CheckPropertyPair(ONProperty firstProperty, ONProperty secondProperty, out ONProperty sourceProperty, out ONProperty targetProperty)
		{
			sourceProperty = null;
			targetProperty = null;

			if (firstProperty.PropertyType == ONPropertyType.Input)
			{
				targetProperty = firstProperty;
			}
			else
			{
				sourceProperty = firstProperty;
			}
			if (secondProperty.PropertyType == ONPropertyType.Input)
			{
				if (targetProperty != null) return false;
				targetProperty = secondProperty;
			}
			else
			{
				if (sourceProperty != null) return false;
				sourceProperty = secondProperty;
			}

			return true;
		}

		public static bool TryLinkProperties(ONProperty sourceProperty, ONProperty targetProperty, out ONLinkType linkType)
		{
			linkType = ONLinkType.Value;

			if (sourceProperty.Node == targetProperty.Node)
				return false;

			var sourceType = sourceProperty.ValueType;
			var targetType = targetProperty.ValueType;

			if (targetType.IsEqualsOrSubclass(typeof(UnityEventBase)))
			{
				linkType = ONLinkType.Event;

				var sourceReflectionMember = sourceProperty.ReflectionMember;
				if (sourceReflectionMember != null && sourceReflectionMember.IsMethod())
				{
					return true;
				}

				return false;
			}

			if (targetType == sourceType || targetType.IsAssignableFrom(sourceType))
			{
				return true;
			}

			try
			{
				if (sourceType != typeof(string))
				{
					var convertedValue = Convert.ChangeType(sourceProperty.GetValue(), targetType);
					return convertedValue != null;
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		#endregion

		#region Static Private Methods

		private static bool AddEvent(ONProperty sourceProperty, ONProperty targetProperty)
		{
			MethodInfo addMethodInfo = null;
			Type eventType = null;

			var targetType = targetProperty.ValueType;

			if (HasAddPersistentMethodInfo(targetType, out addMethodInfo, out eventType))
			{
				var sourceReflectionMember = sourceProperty.ReflectionMember;
				if (sourceReflectionMember != null && sourceReflectionMember.IsMethod())
				{
					var customDelegate = sourceReflectionMember.CreateDelegate(eventType);
					if (customDelegate != null)
					{
						addMethodInfo.Invoke(targetProperty.GetValue(), new object[] { customDelegate });

						return true;
					}
				}
			}

			return false;
		}

		private static void RemoveEvent(ONProperty sourceProperty, ONProperty targetProperty)
		{
			MethodInfo removeMethodInfo = null;

			var targetType = targetProperty.ValueType;

			if (HasRemovePersistentMethodInfo(targetType, out removeMethodInfo))
			{
				var sourceReflectionMember = sourceProperty.ReflectionMember;
				if (sourceReflectionMember != null && sourceReflectionMember.IsMethod())
				{
					var sourceTarget = sourceReflectionMember.Target;
					var sourceMethodInfo = sourceReflectionMember.MemberInfo as MethodInfo;

					removeMethodInfo.Invoke(targetProperty.GetValue(), new object[] { sourceTarget, sourceMethodInfo });
				}
			}
		}

		private static bool HasAddPersistentMethodInfo(Type targetType, out MethodInfo addMethodInfo, out Type eventType)
		{
			addMethodInfo = null;
			eventType = null;

			var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
			var methodsInfos = targetType.GetMethods(bindingFlags);

			foreach (var methodInfo in methodsInfos)
			{
				var parametersInfos = methodInfo.GetParameters();
				if (parametersInfos.Length != 1) continue;

				if (methodInfo.Name == "AddPersistentListener")
				{
					addMethodInfo = methodInfo;
					eventType = parametersInfos[0].ParameterType;

					return true;
				}
			}

			return false;
		}

		private static bool HasRemovePersistentMethodInfo(Type targetType, out MethodInfo removeMethodInfo)
		{
			removeMethodInfo = null;

			var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
			var methodsInfos = targetType.GetMethods(bindingFlags);

			foreach (var methodInfo in methodsInfos)
			{
				var parametersInfos = methodInfo.GetParameters();
				if (parametersInfos.Length != 2) continue;

				if (methodInfo.Name == "RemovePersistentListener")
				{
					removeMethodInfo = methodInfo;

					return true;
				}
			}

			return false;
		}

		#endregion
	}
}