using Unity.SelectionGroups.Runtime;
using UnityEditor;

namespace Unity.SelectionGroups
{
    class SelectionGroupDebugInformation
    {

        public string text;

        public SelectionGroupDebugInformation(ISelectionGroup group)
        {
            text = EditorJsonUtility.ToJson(group, prettyPrint: true);
        }
    }
}