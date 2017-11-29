/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Collections.Generic;

namespace extOpenNodes.Editor
{
    [Serializable]
    public class ONNodeSchema : ScriptableObject
    {
        #region Public Vars

        public string Name
        {
            get { return schemaName; }
            set { schemaName = value; }
        }

        public bool SelfOutput
        {
            get { return selfOutput; }
            set { selfOutput = value; }
        }

        public List<ONPropertySchema> InputPropertiesSchemes
        {
            get { return inputPropertiesSchemes; }
            set { inputPropertiesSchemes = value; }
        }

        public List<ONPropertySchema> OutputPropertiesSchemes
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
        protected string schemaName;

        [SerializeField]
        protected string typeName;

        [SerializeField]
        protected bool selfOutput;

        [SerializeField]
        protected List<ONPropertySchema> inputPropertiesSchemes = new List<ONPropertySchema>();

        [SerializeField]
        protected List<ONPropertySchema> outputPropertiesSchemes = new List<ONPropertySchema>();

        #endregion

        #region Private Vars

        [NonSerialized]
        private bool _customInspector;

        [NonSerialized]
        private Type _type;

        #endregion
    }
}