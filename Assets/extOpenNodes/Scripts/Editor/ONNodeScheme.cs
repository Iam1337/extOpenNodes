/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Collections.Generic;

namespace extOpenNodes.Editor
{
    [Serializable]
    public class ONNodeScheme : ScriptableObject
    {
        #region Public Vars

        public string Name
        {
            get { return schemeName; }
            set { schemeName = value; }
        }

        public bool SelfOutput
        {
            get { return selfOutput; }
            set { selfOutput = value; }
        }

        public List<ONPropertyScheme> InputPropertiesSchemes
        {
            get { return inputPropertiesSchemes; }
            set { inputPropertiesSchemes = value; }
        }

        public List<ONPropertyScheme> OutputPropertiesSchemes
        {
            get { return outputPropertiesSchemes; }
            set { outputPropertiesSchemes = value; }
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
                    _type = Type.GetType(typeName);

                return _type;
            }
            set
            {
                _type = value;
                typeName = _type.AssemblyQualifiedName;
            }
        }

        #endregion

        #region Protected Vars

        [SerializeField]
        protected string schemeName;

        [SerializeField]
        protected string typeName;

        [SerializeField]
        protected bool selfOutput;

        [SerializeField]
        protected List<ONPropertyScheme> inputPropertiesSchemes = new List<ONPropertyScheme>();

        [SerializeField]
        protected List<ONPropertyScheme> outputPropertiesSchemes = new List<ONPropertyScheme>();

        #endregion

        #region Private Vars

        [NonSerialized]
        private bool _customInspector;

        [NonSerialized]
        private Type _type;

        #endregion
    }
}