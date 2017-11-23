/* Copyright (c) 2017 ExT (V.Sigalkin) */

using System;
using System.Reflection;

namespace extOpenNodes.Core
{
    public static class ONUtilities
    {
        #region Static Public Methods

        public static T GetAttribute<T>(MemberInfo memberInfo, bool inherit) where T : Attribute
        {
            return (T)GetAttribute(memberInfo, typeof(T), inherit);
        }

        public static Attribute GetAttribute(MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            if (!attributeType.IsEqualsOrSubclass(typeof(Attribute)))
                throw new Exception("\"attributeType\" is not equals or subclass of Attribute.");

            var attributes = memberInfo.GetCustomAttributes(attributeType, inherit);
            if (attributes.Length > 0)
            {
                return (Attribute)attributes[0];
            }

            return null;
        }

        public static bool IsTypeEqualsOrSubclass(this object sourceTarget, Type targetType)
        {
            return IsEqualsOrSubclass(sourceTarget.GetType(), targetType);
        }

        public static bool IsEqualsOrSubclass(this Type sourceType, Type targetType)
        {
            //TODO: .Net 4.0 Support
            return sourceType.IsSubclassOf(targetType) || sourceType == targetType;
        }

        #endregion
    }
}