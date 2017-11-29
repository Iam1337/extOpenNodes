/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System;
using System.IO;
using System.Linq;
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

            public ONNodeSchema Schema;
        }

        #endregion

        #region Static Public Vars

        public static List<NodeData> NodesDatas
        {
            get { return _nodesDatas; }
        }

        public static List<ONNodeSchema> Schemes
        {
            get { return _schemes; }
        }

        #endregion

        #region Static Private Vars

        private static List<ONNodeSchema> _schemes = new List<ONNodeSchema>();

        private static List<NodeData> _nodesDatas = new List<NodeData>();

        private static string _schemesPath = "Assets/extOpenNodes/Schemes";

        #endregion

        #region Static Public Methods

        static ONNodesUtils()
        {
            ReloadSchemes();
            ReloadNodes();
        }

        public static ONNodeSchema[] GetNodeSchemes(Type componentType)
        {
            var nodeSchemes = new List<ONNodeSchema>();
            if (componentType == null) return nodeSchemes.ToArray();

            var nodeData = _nodesDatas.FirstOrDefault((nd) => nd.ComponentType == componentType);
            if (nodeData != null) nodeSchemes.Add(nodeData.Schema);

            foreach (var nodeSchema in _schemes)
            {
                if (componentType.IsEqualsOrSubclass(nodeSchema.TargetType))
                {
                    nodeSchemes.Add(nodeSchema);
                }
            }

            return nodeSchemes.ToArray();
        }

        public static void ReloadSchemes()
        {
            _schemes.Clear();

            if (!Directory.Exists(_schemesPath))
                Directory.CreateDirectory(_schemesPath);

            var guids = AssetDatabase.FindAssets("t:ONNodeSchema");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                var nodeSchema = AssetDatabase.LoadAssetAtPath<ONNodeSchema>(assetPath);
                if (nodeSchema == null) continue;

                _schemes.Add(nodeSchema);
            }
        }

        public static ONNodeSchema CreateSchema(Type componentType)
        {
            if (!Directory.Exists(_schemesPath))
            {
                Directory.CreateDirectory(_schemesPath);
                AssetDatabase.Refresh();
            }

            var schema = ScriptableObject.CreateInstance<ONNodeSchema>();// new ONNodeSchema();
            schema.Name = ONEditorUtils.TypeFriendlyName(componentType);
            schema.TargetType = componentType;

            var fileName = componentType.FullName + ".asset";
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(_schemesPath + "/" + fileName);

            AssetDatabase.CreateAsset(schema, assetPath);
            AssetDatabase.Refresh();

            Selection.activeObject = schema;

            EditorUtility.FocusProjectWindow();

            _schemes.Add(schema);

            return schema;
        }

        public static void RemoveSchema(ONNodeSchema schema)
        {
            if (_schemes.Contains(schema))
                _schemes.Remove(schema);

            var path = AssetDatabase.GetAssetPath(schema);
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
        }

        public static void RebuildNode(ONWorkflow workflow, ONNode node, ONNodeSchema schema)
        {
            var component = node.Target;
            var componentType = component.GetType();

            if (!componentType.IsEqualsOrSubclass(schema.TargetType))
            {
                return;
            }

            ONWorkflowUtils.RemoveLinks(workflow, node);

            var properties = workflow.GetNodeProperties(node);
            foreach (var property in properties)
            {
                workflow.RemoveProperty(property);
            }

            node.Name = schema.Name;
            node.CustomInspector = schema.CustomInspector;

            if (schema.SelfOutput)
            {
                var property = workflow.CreateProperty(node, ONPropertyType.Output);
                property.Name = "Self";
                property.TargetCast = true;
                property.SortIndex = -1;
            }

            var propertiesSchemes = new List<ONPropertySchema>();
            propertiesSchemes.AddRange(schema.InputPropertiesSchemes);
            propertiesSchemes.AddRange(schema.OutputPropertiesSchemes);

            foreach (var propertySchema in propertiesSchemes)
            {
                var reflectionMember = new ONReflectionMember();
                reflectionMember.Target = component;
                reflectionMember.Member = propertySchema.Member;

                if (!reflectionMember.IsMethod())
                {
                    if ((propertySchema.PropertyType == ONPropertyType.Input && !reflectionMember.CanWrite) ||
                        (propertySchema.PropertyType == ONPropertyType.Output && !reflectionMember.CanRead))
                        continue;
                }

                var property = workflow.CreateProperty(node, propertySchema.PropertyType);
                property.Name = propertySchema.Name;
                property.ReflectionMember = reflectionMember;
                property.SortIndex = propertySchema.SortIndex;
            }
        }

        public static void ReloadNodes()
        {
            _nodesDatas.Clear();

            var dictionary = new Dictionary<string, Type>();
            var guids = AssetDatabase.FindAssets("t:MonoScript");

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
            nodeData.Schema = GenerateSchema(componentType);

            _nodesDatas.Add(nodeData);
        }

        private static ONNodeSchema GenerateSchema(Type componentType)
        {
            var schema = ScriptableObject.CreateInstance<ONNodeSchema>();
            schema.TargetType = componentType;

            var nodeAttribue = ONUtilities.GetAttribute<ONNodeAttribute>(componentType, true);
            if (nodeAttribue != null)
            {
                schema.Name = Path.GetFileName(nodeAttribue.Name);
                schema.CustomInspector = nodeAttribue.CustomInspector;
            }

            var selfAttribute = ONUtilities.GetAttribute<ONSelfOutputAttribute>(componentType, true);
            if (selfAttribute != null)
            {
                schema.SelfOutput = true;
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
                        var propertySchema = new ONPropertySchema();
                        propertySchema.Name = propertyAttribute.Name;
                        propertySchema.Member = memberInfo.Name;
                        propertySchema.SortIndex = propertyAttribute.SortIndex;
                        propertySchema.PropertyType = propertyAttribute.PropertyType;

                        if (propertyAttribute.PropertyType == ONPropertyType.Input)
                        {
                            schema.InputPropertiesSchemes.Add(propertySchema);
                        }
                        else if (propertySchema.PropertyType == ONPropertyType.Output)
                        {
                            schema.OutputPropertiesSchemes.Add(propertySchema);
                        }
                    }
                }
            }

            schema.InputPropertiesSchemes.Sort((x, y) => x.SortIndex.CompareTo(y.SortIndex));
            schema.OutputPropertiesSchemes.Sort((x, y) => x.SortIndex.CompareTo(y.SortIndex));

            return schema;
        }

        #endregion
    }
}
