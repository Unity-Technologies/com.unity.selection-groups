using UnityEditor;

namespace Unity.SelectionGroups
{
    class SelectionGroupDebugInformation
    {

        public string text;

        public SelectionGroupDebugInformation(SelectionGroup group)
        {
            text = EditorJsonUtility.ToJson(group, prettyPrint: true);
        }
    }
}