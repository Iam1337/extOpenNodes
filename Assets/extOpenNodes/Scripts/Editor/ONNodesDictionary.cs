/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using UnityEditor;

using System;
using System.Collections.Generic;

namespace extOpenNodes.Editor
{
    public static class ONNodesDictionary
    {
        #region Static Public Vars

        public static bool UseMonoScripts
        {
            get { return _useMonoScripts; }
            set { _useMonoScripts = value; }
        }

        public static bool UseReflection
        {
            get { return _useReflection; }
            set { _useReflection = value; }
        }

        public static Dictionary<GUIContent, Type> Dictionary
        {
            get
            {
                if (_dictionary.Count == 0)
                {
                    LoadLibrary();
                }

                return _dictionary;
            }
        }

        #endregion

        #region Static Private Vars

        private static bool _useMonoScripts = true;

        private static bool _useReflection;

        private static readonly Dictionary<GUIContent, Type> _dictionary = new Dictionary<GUIContent, Type>();

        #endregion

        #region Static Public Methods

        public static void LoadLibrary()
        {
            var dictionary = new Dictionary<string, Type>();

            if (_useReflection && _useMonoScripts)
            {
                var monoScripts = LoadByMonoScript();
                var reflection = LoadByReflection();

                dictionary = MergeDictionaries(monoScripts, reflection);
            }
            else if (_useMonoScripts)
            {
                dictionary = LoadByMonoScript();
            }
            else if (_useReflection)
            {
                dictionary = LoadByReflection();
            }

            foreach (var pair in dictionary)
            {
                _dictionary.Add(new GUIContent(pair.Key), pair.Value);
            }
        }

        public static Type GetType(string link)
        {
            foreach (var pair in Dictionary)
            {
                if (pair.Key.text == link)
                    return pair.Value;
            }

            return null;
        }

        #endregion

        #region Static Private Methods

        private static Dictionary<string, Type> MergeDictionaries(params Dictionary<string, Type>[] dictionaries)
        {
            var finalDictionary = new Dictionary<string, Type>();

            foreach (var dictionary in dictionaries)
            {
                foreach (var pair in dictionary)
                {
                    if (!finalDictionary.ContainsKey(pair.Key) && !finalDictionary.ContainsValue(pair.Value))
                    {
                        finalDictionary.Add(pair.Key, pair.Value);
                    }
                }
            }

            return finalDictionary;
        }

        private static Dictionary<string, Type> LoadByMonoScript()
        {
            var dictionary = new Dictionary<string, Type>();
            var guids = AssetDatabase.FindAssets("t:MonoScript");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if (monoScript == null) continue;

                var type = monoScript.GetClass();
                if (type == null) continue;

                ProcessType(dictionary, type);
            }

            return dictionary;
        }

        private static Dictionary<string, Type> LoadByReflection()
        {
            var dictionary = new Dictionary<string, Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach(var type in types)
                {
                    ProcessType(dictionary, type);
                }
            }

            return dictionary;
        }

        private static void ProcessType(Dictionary<string, Type> dictionary, Type type)
        {
            if (type.IsAbstract) return;

            var targetAttributes = type.GetCustomAttributes(typeof(ONNodeAttribute), false);
            if (targetAttributes.Length == 0) return;

            foreach (ONNodeAttribute targetAttribute in targetAttributes)
            {
                var name = targetAttribute.Name;
                if (name.EndsWith("/", StringComparison.Ordinal))
                {
                    name = name.Remove(name.Length - 1);
                }

                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, type);
            }
        }

        #endregion
    }
}
