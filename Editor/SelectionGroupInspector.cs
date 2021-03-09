using Unity.SelectionGroups.Runtime;
using UnityEditor;

namespace Unity.SelectionGroupsEditor
{
    [CustomEditor(typeof(Unity.SelectionGroups.Runtime.SelectionGroup))]
    internal class SelectionGroupInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var group = (ISelectionGroup) target;
            var scope = group.Scope;
            var query = group.Query;
            base.OnInspectorGUI();
            if(group.Query != query)
                SelectionGroupManager.ExecuteQuery(group);
            if(group.Scope != scope) 
                SelectionGroupManager.ChangeGroupScope(group, group.Scope);
        }
    }
}