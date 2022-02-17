using System.Collections.Generic;
using System.Reflection;
using Unity.SelectionGroups;
using UnityEditor;

namespace Unity.SelectionGroups.Editor
{
    internal static class SelectionGroupToolAttributeCache 
    {
        private static readonly TypeCache.MethodCollection m_ToolMethods = TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>();        

        private static readonly Dictionary<int, SelectionGroupToolAttribute> m_ToolAttributeMap = new Dictionary<int, SelectionGroupToolAttribute>();
        private static readonly Dictionary<int, MethodInfo> m_ToolMethodInfoMap = new Dictionary<int, MethodInfo>();

        [InitializeOnLoadMethod]
        private static void SelectionGroupToolAttributeCache_OnLoad() 
        {
            m_ToolAttributeMap.Clear();

            foreach (MethodInfo methodInfo in m_ToolMethods) 
            {
                SelectionGroupToolAttribute attr = methodInfo.GetCustomAttribute<SelectionGroupToolAttribute>();
                m_ToolMethodInfoMap[attr.toolId] = methodInfo;
                m_ToolAttributeMap[attr.toolId]  = attr;            
            }
        }
        
        internal static bool TryGetAttribute(int id, out SelectionGroupToolAttribute attribute) 
        {
            return m_ToolAttributeMap.TryGetValue(id, out attribute);            
        }

        internal static bool TryGetMethodInfo(int id, out MethodInfo methodInfo) 
        {
            return m_ToolMethodInfoMap.TryGetValue(id, out methodInfo);            
        }
    }

}