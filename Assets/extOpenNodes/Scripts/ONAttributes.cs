/* Copyright (c) 2017 ExT (V.Sigalkin) */

using System;

using extOpenNodes.Core;

namespace extOpenNodes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ONPropertyAttribute : Attribute
    {
        #region Public Vars

        public string Name
        {
            get { return name; }
        }

        public ONPropertyType PropertyType
        {
            get { return propertyType; }
        }

        public int SortIndex
        {
            get { return sortIndex; }
        }

        #endregion

        #region Protected Vars

        protected string name;

        protected ONPropertyType propertyType;

        protected int sortIndex;

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ONInputAttribute : ONPropertyAttribute
    {
        #region Public Methods

        public ONInputAttribute(int index, string inputName)
        {
            sortIndex = index;
            name = inputName;
            propertyType = ONPropertyType.Input;
        }

        public ONInputAttribute()
        {
            propertyType = ONPropertyType.Input;
            sortIndex = int.MaxValue;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ONOutputAttribute : ONPropertyAttribute
    {
        #region Public Methods

        public ONOutputAttribute(int index, string outputName)
        {
            sortIndex = index;
            name = outputName;
            propertyType = ONPropertyType.Output;
        }

        public ONOutputAttribute()
        {
            propertyType = ONPropertyType.Output;
            sortIndex = int.MaxValue;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ONSelfOutputAttribute : ONPropertyAttribute
    {
        #region Public Methods

        public ONSelfOutputAttribute()
        {
            sortIndex = -1;
            name = "Self";
            propertyType = ONPropertyType.Output;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ONNodeAttribute : Attribute
    {
        #region Public Vars

        public string Name
        {
            get { return name; }
        }

        public bool CustomInspector
        {
            get { return custom; }
        }

        #endregion

        #region Protected Vars

        protected string name;

        protected bool custom;

        #endregion

        #region Public Methods

        public ONNodeAttribute(string nodeName, bool customInspector = false)
        {
            name = nodeName;
            custom = customInspector;
        }

        public ONNodeAttribute()
        { }

        #endregion
    }
}