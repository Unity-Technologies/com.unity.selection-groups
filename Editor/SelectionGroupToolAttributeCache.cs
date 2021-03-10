using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.SelectionGroups;
using UnityEditor;

namespace Unity.SelectionGroupsEditor
{
internal static class SelectionGroupToolAttributeCache {

    [InitializeOnLoadMethod]
    static void SelectionGroupToolAttributeCache_OnLoad() {
        
        m_toolAttributeMap.Clear();

        foreach (MethodInfo methodInfo in m_toolMethods) {
            SelectionGroupToolAttribute attr = methodInfo.GetCustomAttribute<SelectionGroupToolAttribute>();
            m_toolMethodInfoMap[attr.toolId] = methodInfo;
            m_toolAttributeMap[attr.toolId]  = attr;            
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal static bool TryGetAttribute(string id, out SelectionGroupToolAttribute attribute) {
        return m_toolAttributeMap.TryGetValue(id, out attribute);            
    }

    internal static bool TryGetMethodInfo(string id, out MethodInfo methodInfo) {
        return m_toolMethodInfoMap.TryGetValue(id, out methodInfo);            
    }
    

//----------------------------------------------------------------------------------------------------------------------
    
    private static readonly TypeCache.MethodCollection m_toolMethods = TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>();        

    private static readonly Dictionary<string, SelectionGroupToolAttribute> m_toolAttributeMap = new Dictionary<string, SelectionGroupToolAttribute>();
    private static readonly Dictionary<string, MethodInfo> m_toolMethodInfoMap = new Dictionary<string, MethodInfo>();
    

}

} //end namespace