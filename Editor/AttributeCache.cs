using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.SelectionGroups
{
    internal class AttributeCache
    {
        static Dictionary<Type, Dictionary<MethodInfo, Attribute>> attributeMap = new Dictionary<Type, Dictionary<MethodInfo, Attribute>>();

        internal static T GetCustomAttribute<T>(MethodInfo i) where T : Attribute
        {
            if (!attributeMap.TryGetValue(typeof(T), out var map))
                map = attributeMap[typeof(T)] = new Dictionary<MethodInfo, Attribute>();
            if (!map.TryGetValue(i, out var attribute))
                attribute = map[i] = i.GetCustomAttribute<T>();
            return attribute as T;
        }

    }
}