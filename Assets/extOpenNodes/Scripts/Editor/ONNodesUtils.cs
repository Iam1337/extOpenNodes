/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using extOpenNodes.Core;

namespace extOpenNodes.Editor
{
    [InitializeOnLoad]
    public static class ONNodesUtils
    {
        #region Extensions

        public class NodeData
        {
            public Type ComponentType;

            public GUIContent Content;
        }

        #endregion

        #region Static Public Vars

        public static List<NodeData> NodesTypes
        {
            get { return _nodesTypes; }
        }

        #endregion

        #region Static Private Vars

        private static List<NodeData> _nodesTypes = new List<NodeData>();

        private static Dictionary<Type, ONNodeScheme> _generatedSchemes = new Dictionary<Type, ONNodeScheme>();

        private static string _schemesPath = "Assets/extOpenNodes/Schemes";

        #endregion

        #region Static Public Methods

        static ONNodesUtils()
        {
            ReloadNodes();
        }

        public static List<ONNodeScheme> GetSchemes(Type componentType)
        {
			if (componentType == null)
				return new List<ONNodeScheme>();
			
            var schemes = GetSchemesInternal(componentType);

            if (componentType != null && _generatedSchemes.ContainsKey(componentType))
            {
                schemes.Insert(0, _generatedSchemes[componentType]);
            }

            return schemes;
        }

        public static ONNodeScheme CreateScheme(Type componentType)
        {
            if (!Directory.Exists(_schemesPath))
            {
                Directory.CreateDirectory(_schemesPath);
                AssetDatabase.Refresh();
            }

            var scheme = ScriptableObject.CreateInstance<ONNodeScheme>();
            scheme.Name = ONEditorUtils.TypeFriendlyName(componentType);
            scheme.TargetType = componentType;

            var fileName = componentType.FullName + ".asset";
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(_schemesPath + "/" + fileName);

            AssetDatabase.CreateAsset(scheme, assetPath);
            AssetDatabase.Refresh();

            Selection.activeObject = scheme;

            EditorUtility.FocusProjectWindow();

            return scheme;
        }

        public static void RebuildNode(ONWorkflow workflow, ONNode node, ONNodeScheme scheme)
        {
            var component = node.Target;
            var componentType = component.GetType();

            if (!componentType.IsEqualsOrSubclass(scheme.TargetType))
            {
                return;
            }

            ONWorkflowUtils.RemoveLinks(workflow, node);

            var properties = workflow.GetNodeProperties(node);
            foreach (var property in properties)
            {
                workflow.RemoveProperty(property);
            }

            node.Name = scheme.Name;
            node.CustomInspector = scheme.CustomInspector;

            if (scheme.SelfOutput)
            {
                var property = workflow.CreateProperty(node, ONPropertyType.Output);
                property.Name = "Self";
                property.TargetCast = true;
                property.SortIndex = -1;
            }

            var propertiesSchemes = new List<ONPropertyScheme>();
            propertiesSchemes.AddRange(scheme.InputPropertiesSchemes);
            propertiesSchemes.AddRange(scheme.OutputPropertiesSchemes);

            foreach (var propertyScheme in propertiesSchemes)
            {
                var reflectionMember = new ONReflectionMember();
                reflectionMember.Target = component;
                reflectionMember.Member = propertyScheme.Member;

                if (!reflectionMember.IsMethod())
                {
                    if ((propertyScheme.PropertyType == ONPropertyType.Input && !reflectionMember.CanWrite) ||
                        (propertyScheme.PropertyType == ONPropertyType.Output && !reflectionMember.CanRead))
                        continue;
                }

                var property = workflow.CreateProperty(node, propertyScheme.PropertyType);
                property.Name = propertyScheme.Name;
                property.ReflectionMember = reflectionMember;
                property.SortIndex = propertyScheme.SortIndex;
            }
        }

        public static void ReloadNodes()
        {
            _nodesTypes.Clear();

            var dictionary = new Dictionary<string, Type>();
            var guids = AssetDatabase.FindAssets("t:" + typeof(MonoScript).Name);

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if (monoScript == null) continue;

                var componentType = monoScript.GetClass();
                if (componentType == null) continue;

                ProcessType(dictionary, componentType);
            }
        }

        #endregion

        #region Static Private Methods

        private static List<ONNodeScheme> GetSchemesInternal(Type componentType)
        {
            var schemes = new List<ONNodeScheme>();

            if (!Directory.Exists(_schemesPath))
                Directory.CreateDirectory(_schemesPath);

            var guids = AssetDatabase.FindAssets("t:" + typeof(ONNodeScheme).Name);

			foreach (var guid in guids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);

				var scheme = AssetDatabase.LoadAssetAtPath<ONNodeScheme>(assetPath);

				if (scheme == null || scheme.TargetType == null) continue;
				if (!componentType.IsEqualsOrSubclass(scheme.TargetType)) continue;

				schemes.Add(scheme);
			}

            return schemes;
        }

        private static void ProcessType(Dictionary<string, Type> dictionary, Type componentType)
        {
            if (componentType.IsAbstract) return;

            var nodeAttribute = ONUtilities.GetAttribute<ONNodeAttribute>(componentType, false);
            if (nodeAttribute == null) return;

            var name = nodeAttribute.Name;
            if (name.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Remove(name.Length - 1);
            }

            if (dictionary.ContainsKey(name))
                return;

            dictionary.Add(name, componentType);

            var nodeData = new NodeData();
            nodeData.ComponentType = componentType;
            nodeData.Content = new GUIContent(name);

            _generatedSchemes.Add(componentType, GenerateScheme(componentType));
            _nodesTypes.Add(nodeData);
        }

        private static ONNodeScheme GenerateScheme(Type componentType)
        {
            var scheme = ScriptableObject.CreateInstance<ONNodeScheme>();
            scheme.TargetType = componentType;

            var nodeAttribue = ONUtilities.GetAttribute<ONNodeAttribute>(componentType, true);
            if (nodeAttribue != null)
            {
                scheme.Name = Path.GetFileName(nodeAttribue.Name);
                scheme.CustomInspector = nodeAttribue.CustomInspector;
            }

            var selfAttribute = ONUtilities.GetAttribute<ONSelfOutputAttribute>(componentType, true);
            if (selfAttribute != null)
            {
                scheme.SelfOutput = true;
            }

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var membersInfos = componentType.GetMembers(bindingFlags);
            foreach (var memberInfo in membersInfos)
            {
                if (memberInfo is FieldInfo || memberInfo is PropertyInfo || memberInfo is MethodInfo)
                {
                    var methodInfo = memberInfo as MethodInfo;
                    if (methodInfo != null && methodInfo.IsSpecialName)
                    {
                        continue;
                    }

                    var propertiesAttributes = memberInfo.GetCustomAttributes(typeof(ONPropertyAttribute), true);
                    if (propertiesAttributes.Length == 0) continue;

                    foreach (ONPropertyAttribute propertyAttribute in propertiesAttributes)
                    {
                        var propertyScheme = new ONPropertyScheme();
                        propertyScheme.Name = propertyAttribute.Name;
                        propertyScheme.Member = memberInfo.Name;
                        propertyScheme.SortIndex = propertyAttribute.SortIndex;
                        propertyScheme.PropertyType = propertyAttribute.PropertyType;

                        if (propertyAttribute.PropertyType == ONPropertyType.Input)
                        {
                            scheme.InputPropertiesSchemes.Add(propertyScheme);
                        }
                        else if (propertyScheme.PropertyType == ONPropertyType.Output)
                        {
                            scheme.OutputPropertiesSchemes.Add(propertyScheme);
                        }
                    }
                }
            }

            scheme.InputPropertiesSchemes.Sort((x, y) => x.SortIndex.CompareTo(y.SortIndex));
            scheme.OutputPropertiesSchemes.Sort((x, y) => x.SortIndex.CompareTo(y.SortIndex));

            return scheme;
        }

        #endregion
    }
}
