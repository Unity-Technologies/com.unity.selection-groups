using Unity.SelectionGroups;
using UnityEditor;

namespace Unity.SelectionGroups.Editor
{
    class SelectionGroupDebugInformation
    {

        public string text;

        internal SelectionGroupDebugInformation(ISelectionGroup group)
        {
            text = EditorJsonUtility.ToJson(group, prettyPrint: true);
        }
    }
}