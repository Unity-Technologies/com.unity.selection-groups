using System.Collections.Generic;
using System.Reflection;
using Unity.FilmInternalUtilities;
using Unity.SelectionGroups;
using UnityEditor;

namespace Unity.SelectionGroups.Editor
{
internal static class SelectionGroupToolAttributeCache {

    [InitializeOnLoadMethod]
    static void SelectionGroupToolAttributeCache_OnLoad() {
        
        m_toolAttributeMap.Clear();

        m_toolMethods.Loop((MethodInfo methodInfo) => {
            SelectionGroupToolAttribute attr = methodInfo.GetCustomAttribute<SelectionGroupToolAttribute>();
            m_toolMethodInfoMap[attr.toolId] = methodInfo;
            m_toolAttributeMap[attr.toolId]  = attr;
        });
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal static bool TryGetAttribute(int id, out SelectionGroupToolAttribute attribute) {
        return m_toolAttributeMap.TryGetValue(id, out attribute);            
    }

    internal static bool TryGetMethodInfo(int id, out MethodInfo methodInfo) {
        return m_toolMethodInfoMap.TryGetValue(id, out methodInfo);            
    }
    

//----------------------------------------------------------------------------------------------------------------------
    
    private static readonly TypeCache.MethodCollection m_toolMethods = TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>();        

    private static readonly Dictionary<int, SelectionGroupToolAttribute> m_toolAttributeMap = new Dictionary<int, SelectionGroupToolAttribute>();
    private static readonly Dictionary<int, MethodInfo> m_toolMethodInfoMap = new Dictionary<int, MethodInfo>();
    

}

} //end namespace