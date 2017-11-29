/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using System;

using extOpenNodes.Core;

namespace extOpenNodes.Editor
{
    [Serializable]
    public class ONPropertySchema
    {
        #region Public Vars

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Member
        {
            get { return member; }
            set { member = value; }
        }

        public int SortIndex
        {
            get { return _sortIndex; }
            set { _sortIndex = value; }
        }

        public ONPropertyType PropertyType
        {
            get { return propertyType; }
            set { propertyType = value; }
        }

        #endregion

        #region Protected Vars 

        [SerializeField]
        protected string name;

        [SerializeField]
        protected string member;

        [SerializeField]
        protected string typeName;

        [SerializeField]
        protected ONPropertyType propertyType;

        #endregion

        #region Private Vars

        [NonSerialized]
        private int _sortIndex;

        #endregion

    }
}